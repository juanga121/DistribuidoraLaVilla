using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Inventario;
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
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosRepository;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CrearMovimientoProductoDTOValidator _validator;

        public MovimientosProductosService(
            IGenericRepository<MovimientosProductosEntity, int> movimientosRepository,
            IGenericRepository<LotesProductosEntity, int> lotesRepository,
            IGenericRepository<ProductosEntity, int> productosRepository,
            IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork)
        {
            _movimientosRepository = movimientosRepository;
            _lotesRepository = lotesRepository;
            _productosRepository = productosRepository;
            _auditoriaService = auditoriaService;
            _unitOfWork = unitOfWork;
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

            bool ownTransaction = !_unitOfWork.HasActiveTransaction;
            if (ownTransaction)
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // The lot's authoritative counter is expressed in the product's mode unit, so a movement
                // simply adds to or subtracts from that single counter.
                var producto = await _productosRepository.FindByIdAsync(lote.IdProducto)
                    ?? throw new InvalidOperationException(
                        $"No se encontró el producto con ID {lote.IdProducto} asociado al lote {lote.Id}");

                var operaPorPeso = ProductoStockRules.OperaPorPeso(producto);

                // The movement quantity is always expressed in the product's mode unit: kilograms for
                // weight modes, units for unit mode. The client-supplied unit of measure is only a
                // convenience and must agree with the mode; a mismatch is a caller bug, not a conversion.
                var unidadEsperada = ProductoStockRules.UnidadMedidaId(operaPorPeso);

                if (dto.IdUnidadMedida > 0 && dto.IdUnidadMedida != unidadEsperada)
                {
                    throw new InvalidOperationException(
                        $"El producto '{producto.Nombre}' administra su stock en " +
                        $"{(operaPorPeso ? "kilogramos" : "unidades")}, por lo que el movimiento debe informarse en esa unidad");
                }

                var stockAnterior = lote.CantidadDisponible;
                var nuevoStock = CalcularNuevoStock(
                    stockAnterior,
                    dto.Cantidad,
                    (TipoMovimientoProducto)dto.TipoMovimiento
                );

                if (nuevoStock < 0)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente. Disponible: {stockAnterior}, Requerido: {dto.Cantidad}. " +
                        $"Faltante: {Math.Abs(nuevoStock)}"
                    );
                }

                // In weight modes the authoritative counter is already kilograms; in unit mode the lot
                // does not track kilograms at all.
                var nuevoPeso = operaPorPeso ? nuevoStock : 0m;

                // Value the movement with the cost matching the unit the lot is measured in.
                var costoUnitario = operaPorPeso ? lote.PrecioKilo : lote.PrecioUnitario;
                var totalMovimiento = dto.Cantidad * costoUnitario;

                var movimiento = new MovimientosProductosEntity
                {
                    IdLoteProducto = dto.IdLoteProducto,
                    TipoMovimiento = dto.TipoMovimiento,
                    FechaMovimiento = DateTime.Now,
                    Cantidad = dto.Cantidad,
                    TotalMovimiento = totalMovimiento,
                    IdUnidadMedida = unidadEsperada,
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

                if (ownTransaction)
                    await _unitOfWork.CommitAsync();

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
            catch
            {
                if (ownTransaction)
                    await _unitOfWork.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork.DisposeAsync();
            }
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
