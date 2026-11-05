using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Inventario;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class LotesProductosService(
        IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
        IGenericRepository<MovimientosProductosEntity, int> movimientosProductosRepository,
        IGenericRepository<ProductosEntity, int> productosRepository,
        IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
        IGenericRepository<MarcasEntity, int> marcasRepository,
        IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
        IAuditoriaService auditoriaService,
        IUnitOfWork? unitOfWork = null)
    {
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository = lotesProductosRepository;
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosProductosRepository = movimientosProductosRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository = productosRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository = unidadMedidaRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;
        private readonly IUnitOfWork? _unitOfWork = unitOfWork;

        public async Task CrearLoteProductoAsync(LotesProductosDTO lotesProductosDTO)
        {
            var validator = new LotesProductosDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesProductosDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            bool ownTransaction = _unitOfWork != null && !_unitOfWork.HasActiveTransaction;
            if (ownTransaction)
                await _unitOfWork!.BeginTransactionAsync();

            try
            {
                var producto = await _productosRepository.FindByIdAsync(lotesProductosDTO.IdProducto)
                    ?? throw new InvalidOperationException(
                        $"No se encontró el producto con ID {lotesProductosDTO.IdProducto}");

                var operaPorPeso = ProductoStockRules.OperaPorPeso(producto);
                ValidarCantidadesPorModo(operaPorPeso, lotesProductosDTO);
                var precioTotal = ProductoStockRules.CalcularImporte(
                    operaPorPeso,
                    lotesProductosDTO.CantidadUnidades,
                    lotesProductosDTO.PesoTotal,
                    lotesProductosDTO.PrecioUnitario,
                    lotesProductosDTO.PrecioKilo);

                // The lot's authoritative counter always holds the stock in the product's mode unit:
                // kilograms for weight modes, units for unit mode.
                var cantidadDisponible = ProductoStockRules.CantidadAutoritativa(
                    operaPorPeso,
                    lotesProductosDTO.CantidadUnidades,
                    lotesProductosDTO.PesoTotal);
                var pesoDisponible = operaPorPeso ? lotesProductosDTO.PesoTotal : 0m;

                LotesProductosEntity entity = new()
                {
                    IdProducto = lotesProductosDTO.IdProducto,
                    IdProveedor = lotesProductosDTO.IdProveedor,
                    FechaEntrada = DateTime.Now,
                    FechaVencimiento = lotesProductosDTO.FechaVencimiento,
                    CantidadUnidades = lotesProductosDTO.CantidadUnidades,
                    PesoTotal = operaPorPeso ? lotesProductosDTO.PesoTotal : 0m,
                    IdUnidadMedida = ProductoStockRules.UnidadMedidaId(operaPorPeso),
                    PrecioUnitario = operaPorPeso ? 0m : lotesProductosDTO.PrecioUnitario,
                    PrecioKilo = operaPorPeso ? lotesProductosDTO.PrecioKilo : 0m,
                    PrecioTotal = precioTotal,
                    IdMarca = lotesProductosDTO.IdMarca,
                    CantidadInicial = cantidadDisponible,
                    CantidadDisponible = cantidadDisponible,
                    PesoDisponible = pesoDisponible,
                    Estado = 1
                };
                await _lotesProductosRepository.CreateAsync(entity);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        idProducto = entity.IdProducto,
                        cantidadUnidades = entity.CantidadUnidades,
                        pesoTotal = entity.PesoTotal,
                        precioTotal
                    });
                    await _auditoriaService.RegistrarAsync("StockProducto", entity.Id.ToString(), "CrearLote", detalle, lotesProductosDTO.IdUsuario);
                }
                catch { /* fire-and-forget */ }

                var movimiento = new MovimientosProductosEntity
                {
                    IdLoteProducto = entity.Id,
                    TipoMovimiento = (int)TipoMovimientoProducto.Entrada,
                    FechaMovimiento = DateTime.Now,
                    Cantidad = cantidadDisponible,
                    TotalMovimiento = precioTotal,
                    IdUnidadMedida = ProductoStockRules.UnidadMedidaId(operaPorPeso),
                    IdUsuario = lotesProductosDTO.IdUsuario,
                    Observacion = operaPorPeso
                        ? $"Ingreso de lote - {lotesProductosDTO.PesoTotal} kg"
                        : $"Ingreso de lote - {lotesProductosDTO.CantidadUnidades} unidades",
                    Estado = 1
                };
                await _movimientosProductosRepository.CreateAsync(movimiento);

                if (ownTransaction)
                    await _unitOfWork!.CommitAsync();
            }
            catch
            {
                if (ownTransaction)
                    await _unitOfWork!.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork!.DisposeAsync();
            }
        }

        public async Task<List<LotesProductosResponseDTO>> ObtenerLotesProductosAsync()
        {
            var lotes = await _lotesProductosRepository.GetAllAsync();
            return await MapearARespuestaAsync(lotes);
        }

        public async Task<LotesProductosResponseDTO> ObtenerLoteProductoPorIdAsync(int id)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(id);
            if (lote == null)
                throw new Exception("El lote de producto no existe");

            var resultado = await MapearARespuestaAsync([lote]);
            return resultado.First();
        }

        public async Task ActualizarEstadoLoteProducto(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (lote == null)
                throw new Exception("El lote de producto no existe");

            bool ownTransaction = _unitOfWork != null && !_unitOfWork.HasActiveTransaction;
            if (ownTransaction)
                await _unitOfWork!.BeginTransactionAsync();

            try
            {
                var estadoAnterior = lote.Estado;
                lote.Estado = actualizarEstadoDTO.EstadoNuevo;
                await _lotesProductosRepository.UpdateAsync(lote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        estadoAnterior,
                        estadoNuevo = lote.Estado
                    });
                    await _auditoriaService.RegistrarAsync("StockProducto", lote.Id.ToString(), "CambioEstado", detalle, actualizarEstadoDTO.IdUsuario ?? Guid.Empty);
                }
                catch { /* fire-and-forget */ }

                if (actualizarEstadoDTO.EstadoNuevo == 0 && estadoAnterior != 0)
                {
                    var movimiento = new MovimientosProductosEntity
                    {
                        IdLoteProducto = lote.Id,
                        TipoMovimiento = (int)TipoMovimientoProducto.Vencimiento,
                        FechaMovimiento = DateTime.Now,
                        Cantidad = lote.CantidadDisponible,
                        TotalMovimiento = lote.PrecioTotal,
                        IdUnidadMedida = lote.IdUnidadMedida,
                        IdUsuario = actualizarEstadoDTO.IdUsuario ?? Guid.Empty,
                        Observacion = $"Baja de lote por vencimiento - {lote.CantidadDisponible} unidades",
                        Estado = 1
                    };
                    await _movimientosProductosRepository.CreateAsync(movimiento);
                }

                if (ownTransaction)
                    await _unitOfWork!.CommitAsync();
            }
            catch
            {
                if (ownTransaction)
                    await _unitOfWork!.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork!.DisposeAsync();
            }
        }

        public async Task ActualizarLoteProducto(int id, LotesProductosDTO lotesProductosDTO)
        {
            var validator = new LotesProductosDTOValidator(validarFechaVencimientoFutura: false);
            var validationResult = await validator.ValidateAsync(lotesProductosDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

var lote = await _lotesProductosRepository.FindByIdAsync(id);
            if (lote == null)
                throw new Exception("El lote de producto no existe");

            bool ownTransaction = _unitOfWork != null && !_unitOfWork.HasActiveTransaction;
            if (ownTransaction)
                await _unitOfWork!.BeginTransactionAsync();

            try
            {
                var producto = await _productosRepository.FindByIdAsync(lote.IdProducto)
                    ?? throw new InvalidOperationException(
                        $"No se encontró el producto con ID {lote.IdProducto} asociado al lote {id}");

                if (lotesProductosDTO.IdProducto != lote.IdProducto)
                {
                    throw new InvalidOperationException("No se puede cambiar el producto de un lote existente");
                }

                var operaPorPeso = ProductoStockRules.OperaPorPeso(producto);
                ValidarCantidadesPorModo(operaPorPeso, lotesProductosDTO);

                // The authoritative counter is always in the product's mode unit, so the requested total
                // and the already-consumed quantity are compared in that same unit. Legacy lots hold their
                // initial quantity and their available quantity on the same scale, so their difference is
                // the consumed amount.
                var nuevoTotal = ProductoStockRules.CantidadAutoritativa(
                    operaPorPeso,
                    lotesProductosDTO.CantidadUnidades,
                    lotesProductosDTO.PesoTotal);
                var cantidadConsumida = lote.CantidadInicial - lote.CantidadDisponible;

                if (nuevoTotal < cantidadConsumida)
                {
                    throw new InvalidOperationException(
                        "No se puede reducir el lote por debajo de lo ya consumido");
                }

                var nuevaCantidadDisponible = nuevoTotal - cantidadConsumida;

                lote.IdProveedor = lotesProductosDTO.IdProveedor;
                lote.FechaVencimiento = lotesProductosDTO.FechaVencimiento;
                lote.CantidadUnidades = lotesProductosDTO.CantidadUnidades;
                lote.PesoTotal = operaPorPeso ? nuevoTotal : 0m;
                lote.IdUnidadMedida = ProductoStockRules.UnidadMedidaId(operaPorPeso);
                lote.PrecioUnitario = operaPorPeso ? 0m : lotesProductosDTO.PrecioUnitario;
                lote.PrecioKilo = operaPorPeso ? lotesProductosDTO.PrecioKilo : 0m;
                lote.PrecioTotal = ProductoStockRules.CalcularImporte(
                    operaPorPeso,
                    lotesProductosDTO.CantidadUnidades,
                    lotesProductosDTO.PesoTotal,
                    lotesProductosDTO.PrecioUnitario,
                    lotesProductosDTO.PrecioKilo);
                lote.IdMarca = lotesProductosDTO.IdMarca;

                lote.CantidadInicial = nuevoTotal;
                lote.CantidadDisponible = nuevaCantidadDisponible;
                lote.PesoDisponible = operaPorPeso ? nuevaCantidadDisponible : 0m;

                await _lotesProductosRepository.UpdateAsync(lote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        cantidadUnidades = lotesProductosDTO.CantidadUnidades,
                        pesoTotal = lotesProductosDTO.PesoTotal,
                        totalAutoritativo = nuevoTotal,
                        cantidadConsumida,
                        cantidadDisponible = nuevaCantidadDisponible
                    });
                    await _auditoriaService.RegistrarAsync("StockProducto", id.ToString(), "Modificar", detalle, lotesProductosDTO.IdUsuario);
                }
                catch { /* fire-and-forget */ }

                if (ownTransaction)
                    await _unitOfWork!.CommitAsync();
            }
            catch
            {
                if (ownTransaction)
                    await _unitOfWork!.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork!.DisposeAsync();
            }
        }

        public async Task<List<LotesProductosResponseDTO>> ObtenerLotesProductosDisponiblesAsync()
        {
            var lotes = await _lotesProductosRepository.GetAllAsync();
            return await MapearARespuestaAsync([.. lotes.Where(l => l.Estado == 1)]);
        }

        public async Task EliminarLoteProductoAsync(int idLote, Guid idUsuario)
        {
            var existente = await _lotesProductosRepository.FindByIdAsync(idLote);
            if (existente == null)
                throw new Exception("El lote de producto no existe");

            bool ownTransaction = _unitOfWork != null && !_unitOfWork.HasActiveTransaction;
            if (ownTransaction)
                await _unitOfWork!.BeginTransactionAsync();

            try
            {
                var movimiento = new MovimientosProductosEntity
                {
                    IdLoteProducto = existente.Id,
                    TipoMovimiento = (int)TipoMovimientoProducto.Ajuste,
                    FechaMovimiento = DateTime.Now,
                    Cantidad = existente.CantidadDisponible,
                    TotalMovimiento = existente.PrecioTotal,
                    IdUnidadMedida = existente.IdUnidadMedida,
                    IdUsuario = idUsuario,
                    Observacion = existente.IdUnidadMedida == ProductoStockRules.IdUnidadMedidaKilo
                        ? $"Eliminación de lote - {existente.CantidadDisponible} kg"
                        : $"Eliminación de lote - {existente.CantidadDisponible} unidades",
                    Estado = 1
                };
                await _movimientosProductosRepository.CreateAsync(movimiento);

                await _lotesProductosRepository.DeleteAsync(idLote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        cantidadDisponible = existente.CantidadDisponible
                    });
                    await _auditoriaService.RegistrarAsync("StockProducto", idLote.ToString(), "Eliminar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }

                if (ownTransaction)
                    await _unitOfWork!.CommitAsync();
            }
            catch
            {
                if (ownTransaction)
                    await _unitOfWork!.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork!.DisposeAsync();
            }
        }

        /// <summary>
        /// Requires only the quantities and prices that the product's inventory mode actually uses.
        /// Weight modes work with kilograms and a cost per kilogram; unit mode works with units and a
        /// cost per unit. The unused values are ignored.
        /// </summary>
        private static void ValidarCantidadesPorModo(bool operaPorPeso, LotesProductosDTO dto)
        {
            if (operaPorPeso)
            {
                if (dto.PesoTotal <= 0)
                    throw new ValidationException("El peso total debe ser mayor a 0 para un producto que se administra por peso");

                if (dto.PrecioKilo <= 0)
                    throw new ValidationException("El costo por kilo debe ser mayor a 0 para un producto que se administra por peso");

                return;
            }

            if (dto.CantidadUnidades <= 0)
                throw new ValidationException("La cantidad de unidades debe ser mayor a 0 para un producto que se administra por unidad");

            if (dto.PrecioUnitario <= 0)
                throw new ValidationException("El costo unitario debe ser mayor a 0 para un producto que se administra por unidad");
        }

        private async Task<List<LotesProductosResponseDTO>> MapearARespuestaAsync(List<LotesProductosEntity> lotes)
        {
            var productos = await _productosRepository.GetAllAsync();
            var proveedores = await _proveedoresRepository.GetAllAsync();
            var marcas = await _marcasRepository.GetAllAsync();
            var unidades = await _unidadMedidaRepository.GetAllAsync();

            var productosDict = productos.ToDictionary(p => p.Id);
            var proveedoresDict = proveedores.ToDictionary(p => p.IdProveedor);
            var marcasDict = marcas.ToDictionary(m => m.IdMarca);
            var unidadesDict = unidades.ToDictionary(u => u.Id);

            return lotes.Select(l => new LotesProductosResponseDTO
            {
                Id = l.Id,
                IdProducto = l.IdProducto,
                NombreProducto = productosDict.ContainsKey(l.IdProducto) ? productosDict[l.IdProducto].Nombre : "N/A",
                IdProveedor = l.IdProveedor,
                NombreProveedor = proveedoresDict.ContainsKey(l.IdProveedor) ? proveedoresDict[l.IdProveedor].Nombre : "N/A",
                FechaEntrada = l.FechaEntrada,
                FechaVencimiento = l.FechaVencimiento,
                CantidadUnidades = l.CantidadUnidades,
                PesoTotal = l.PesoTotal,
                IdUnidadMedida = l.IdUnidadMedida,
                NombreUnidadMedida = unidadesDict.ContainsKey(l.IdUnidadMedida) ? unidadesDict[l.IdUnidadMedida].Nombre : "N/A",
                SimboloUnidadMedida = unidadesDict.ContainsKey(l.IdUnidadMedida) ? unidadesDict[l.IdUnidadMedida].Abreviatura : "N/A",
                PrecioUnitario = l.PrecioUnitario,
                PrecioKilo = l.PrecioKilo,
                PrecioTotal = l.PrecioTotal,
                IdMarca = l.IdMarca,
                NombreMarca = marcasDict.ContainsKey(l.IdMarca) ? marcasDict[l.IdMarca].Nombre : "N/A",
                CantidadInicial = l.CantidadInicial,
                CantidadDisponible = l.CantidadDisponible,
                PesoDisponible = l.PesoDisponible,
                ModoInventario = productosDict.ContainsKey(l.IdProducto)
                    ? ProductoStockRules.ResolverModo(productosDict[l.IdProducto].ModoInventario, productosDict[l.IdProducto].VentaPorPeso)
                    : (int)ModoInventarioEnum.Unidad,
                Estado = l.Estado
            }).ToList();
        }
    }
}
