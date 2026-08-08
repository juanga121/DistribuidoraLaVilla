using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class MovimientosProductosService
    {
        /// <summary>Canonical unit-of-measure ids shared across the inventory flows.</summary>
        private const int IdUnidadMedidaKilo = 1;

        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosRepository;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;
        private readonly CrearMovimientoProductoDTOValidator _validator;
        private readonly IAuditoriaService _auditoriaService;

        public MovimientosProductosService(
            IGenericRepository<MovimientosProductosEntity, int> movimientosRepository,
            IGenericRepository<LotesProductosEntity, int> lotesRepository,
            IGenericRepository<ProductosEntity, int> productosRepository,
            IAuditoriaService auditoriaService)
        {
            _movimientosRepository = movimientosRepository;
            _lotesRepository = lotesRepository;
            _productosRepository = productosRepository;
            _auditoriaService = auditoriaService;
            _validator = new CrearMovimientoProductoDTOValidator();
        }

        /// <summary>
        /// Registra un nuevo movimiento de producto y actualiza el stock del lote
        /// </summary>
        public async Task<RespuestaMovimientoProductoDTO> RegistrarMovimientoAsync(CrearMovimientoProductoDTO dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            var lote = await _lotesRepository.FindByIdAsync(dto.IdLoteProducto);
            if (lote == null)
            {
                throw new InvalidOperationException($"No se encontró el lote de producto con ID {dto.IdLoteProducto}");
            }

            if (lote.Estado != 1)
            {
                throw new InvalidOperationException($"El lote {dto.IdLoteProducto} no está disponible (Estado: {lote.Estado})");
            }

            // A product with a weight per unit is consumed in dual mode: InventarioService
            // converts between units and weight on every sale, so the lot must always satisfy
            // PesoDisponible == CantidadDisponible * PesoPorUnidad. Moving only one counter
            // desyncs the lot and can strand stock, because lot selection filters on
            // CantidadDisponible > 0.
            var producto = await _productosRepository.FindByIdAsync(lote.IdProducto)
                ?? throw new InvalidOperationException(
                    $"No se encontró el producto con ID {lote.IdProducto} asociado al lote {lote.Id}");

            var pesoPorUnidad = producto.PesoPorUnidad;
            var tieneConversionPorPeso = pesoPorUnidad.HasValue && pesoPorUnidad.Value > 0;
            var pesoPorUnidadLote = tieneConversionPorPeso && lote.CantidadDisponible > 0
                ? lote.PesoDisponible / lote.CantidadDisponible
                : 0m;

            // The movement is expressed either in kilos or in units. Both counters are derived
            // from that single quantity so the invariant survives the movement.
            var movimientoEnPeso = tieneConversionPorPeso && dto.IdUnidadMedida == IdUnidadMedidaKilo;

            if (!tieneConversionPorPeso && dto.IdUnidadMedida == IdUnidadMedidaKilo)
            {
                throw new InvalidOperationException(
                    $"El producto '{producto.Nombre}' no tiene peso por unidad definido, " +
                    "por lo que no admite movimientos expresados en kilos");
            }

            if (tieneConversionPorPeso && pesoPorUnidadLote <= 0)
            {
                throw new InvalidOperationException(
                    $"El lote {lote.Id} del producto '{producto.Nombre}' tiene una relación peso/unidad inválida");
            }

            var cantidadEnUnidades = movimientoEnPeso
                ? dto.Cantidad / pesoPorUnidadLote
                : dto.Cantidad;

            var stockAnterior = lote.CantidadDisponible;
            var nuevoStock = CalcularNuevoStock(
                stockAnterior,
                cantidadEnUnidades,
                (TipoMovimientoProducto)dto.TipoMovimiento
            );

            if (nuevoStock < 0)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente. Disponible: {stockAnterior}, Requerido: {cantidadEnUnidades}. " +
                    $"Faltante: {Math.Abs(nuevoStock)}"
                );
            }

            // Weight mirrors units through the same ratio, so both counters stay consistent.
            var nuevoPeso = tieneConversionPorPeso
                ? nuevoStock * pesoPorUnidadLote
                : nuevoStock;

            // Value the movement with the cost matching the unit it was expressed in,
            // instead of always charging PrecioKilo.
            var costoUnitario = movimientoEnPeso ? lote.PrecioKilo : lote.PrecioUnitario;
            var totalMovimiento = dto.Cantidad * costoUnitario;

            var movimiento = new MovimientosProductosEntity
            {
                IdLoteProducto = dto.IdLoteProducto,
                TipoMovimiento = dto.TipoMovimiento,
                FechaMovimiento = DateTime.Now,
                Cantidad = dto.Cantidad,
                TotalMovimiento = totalMovimiento,
                IdUnidadMedida = dto.IdUnidadMedida,
                IdCliente = dto.IdCliente,
                IdProveedor = dto.IdProveedor,
                IdUsuario = dto.IdUsuario,
                Observacion = dto.Observacion,
                Estado = 1
            };

            lote.CantidadDisponible = nuevoStock;
            lote.PesoDisponible = nuevoPeso;

            await _movimientosRepository.CreateAsync(movimiento);
            await _lotesRepository.UpdateAsync(lote);

            try
            {
                var detalle = JsonSerializer.Serialize(new
                {
                    tipoMovimiento = ObtenerNombreTipoMovimiento(movimiento.TipoMovimiento),
                    cantidad = movimiento.Cantidad,
                    stockAnterior,
                    stockNuevo = nuevoStock,
                    observacion = movimiento.Observacion
                });
                await _auditoriaService.RegistrarAsync("MovimientoProducto", movimiento.Id.ToString(), "Crear", detalle, dto.IdUsuario);
            }
            catch { /* fire-and-forget */ }

            return new RespuestaMovimientoProductoDTO
            {
                IdMovimiento = movimiento.Id,
                IdLoteProducto = movimiento.IdLoteProducto,
                TipoMovimiento = movimiento.TipoMovimiento,
                TipoMovimientoNombre = ObtenerNombreTipoMovimiento(movimiento.TipoMovimiento),
                FechaMovimiento = movimiento.FechaMovimiento,
                Cantidad = movimiento.Cantidad,
                TotalMovimiento = movimiento.TotalMovimiento,
                IdUnidadMedida = movimiento.IdUnidadMedida,
                IdCliente = movimiento.IdCliente,
                IdProveedor = movimiento.IdProveedor,
                IdUsuario = movimiento.IdUsuario,
                Observacion = movimiento.Observacion,
                Estado = movimiento.Estado,
                StockAnterior = stockAnterior,
                StockNuevo = nuevoStock
            };
        }

        /// <summary>
        /// Calcula el nuevo stock según el tipo de movimiento
        /// </summary>
        private decimal CalcularNuevoStock(decimal stockActual, decimal cantidad, TipoMovimientoProducto tipo)
        {
            return tipo switch
            {
                TipoMovimientoProducto.Entrada => stockActual + cantidad,
                TipoMovimientoProducto.Venta => stockActual - cantidad,
                TipoMovimientoProducto.Ajuste => cantidad,
                TipoMovimientoProducto.Devolucion => stockActual + cantidad,
                TipoMovimientoProducto.Vencimiento => stockActual - cantidad,
                _ => throw new InvalidOperationException($"Tipo de movimiento no válido: {tipo}")
            };
        }

        /// <summary>
        /// Obtiene el nombre descriptivo del tipo de movimiento
        /// </summary>
        private string ObtenerNombreTipoMovimiento(int tipoMovimiento)
        {
            return tipoMovimiento switch
            {
                1 => "Entrada",
                2 => "Venta",
                3 => "Ajuste",
                4 => "Devolución",
                5 => "Vencimiento",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Obtiene todos los movimientos de productos
        /// </summary>
        public async Task<List<MovimientosProductosEntity>> ObtenerMovimientosAsync()
        {
            return await _movimientosRepository.GetAllAsync();
        }

        /// <summary>
        /// Obtiene un movimiento por ID
        /// </summary>
        public async Task<MovimientosProductosEntity?> ObtenerMovimientoPorIdAsync(int id)
        {
            return await _movimientosRepository.FindByIdAsync(id);
        }

        /// <summary>
        /// Obtiene movimientos de un lote específico
        /// </summary>
        public List<MovimientosProductosEntity> ObtenerMovimientosPorLote(int idLote)
        {
            return _movimientosRepository.GetByFilter(m => m.IdLoteProducto == idLote);
        }

        /// <summary>
        /// Obtiene movimientos filtrados por tipo
        /// </summary>
        public List<MovimientosProductosEntity> ObtenerMovimientosPorTipo(int tipoMovimiento)
        {
            return _movimientosRepository.GetByFilter(m => m.TipoMovimiento == tipoMovimiento);
        }

        /// <summary>
        /// Obtiene movimientos en un rango de fechas
        /// </summary>
        public List<MovimientosProductosEntity> ObtenerMovimientosPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var finDelDia = fechaFin.Date.AddDays(1).AddTicks(-1);
            return _movimientosRepository.GetByFilter(m =>
                m.FechaMovimiento >= fechaInicio && m.FechaMovimiento <= finDelDia);
        }

        /// <summary>
        /// Obtiene movimientos por cliente
        /// </summary>
        public List<MovimientosProductosEntity> ObtenerMovimientosPorCliente(Guid idCliente)
        {
            return _movimientosRepository.GetByFilter(m => m.IdCliente == idCliente);
        }

        /// <summary>
        /// Obtiene movimientos por proveedor
        /// </summary>
        public List<MovimientosProductosEntity> ObtenerMovimientosPorProveedor(Guid idProveedor)
        {
            return _movimientosRepository.GetByFilter(m => m.IdProveedor == idProveedor);
        }

        /// <summary>
        /// Actualiza el estado de un movimiento (cancelación, anulación, etc.)
        /// </summary>
        public async Task<bool> ActualizarEstadoMovimientoAsync(int id, int nuevoEstado)
        {
            var movimiento = await _movimientosRepository.FindByIdAsync(id);
            if (movimiento == null)
            {
                return false;
            }

            movimiento.Estado = nuevoEstado;
            await _movimientosRepository.UpdateAsync(movimiento);
            return true;
        }

        /// <summary>
        /// Elimina (desactiva) un movimiento
        /// </summary>
        public async Task<bool> EliminarMovimientoAsync(int id)
        {
            var movimiento = await _movimientosRepository.FindByIdAsync(id);
            if (movimiento == null)
            {
                return false;
            }

            movimiento.Estado = 0;
            await _movimientosRepository.UpdateAsync(movimiento);
            return true;
        }

        /// <summary>
        /// Obtiene un catálogo de tipos de movimiento
        /// </summary>
        public List<object> ObtenerTiposMovimiento()
        {
            return new List<object>
            {
                new { Id = 1, Nombre = "Entrada", Descripcion = "Entrada de inventario desde proveedor" },
                new { Id = 2, Nombre = "Venta", Descripcion = "Venta a cliente (descuenta stock)" },
                new { Id = 3, Nombre = "Ajuste", Descripcion = "Ajuste manual de inventario" },
                new { Id = 4, Nombre = "Devolución", Descripcion = "Devolución de cliente (suma stock)" },
                new { Id = 5, Nombre = "Vencimiento", Descripcion = "Producto vencido (descuenta stock)" }
            };
        }
    }
}
