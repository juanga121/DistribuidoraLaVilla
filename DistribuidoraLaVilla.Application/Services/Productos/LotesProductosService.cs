using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
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
                var precioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);

                var producto = await _productosRepository.FindByIdAsync(lotesProductosDTO.IdProducto);
                var pesoPorUnidad = producto?.PesoPorUnidad;

                decimal cantidadDisponible;
                decimal pesoDisponible;
                if (pesoPorUnidad.HasValue)
                {
                    cantidadDisponible = lotesProductosDTO.CantidadUnidades;
                    pesoDisponible = lotesProductosDTO.CantidadUnidades * pesoPorUnidad.Value;
                }
                else
                {
                    cantidadDisponible = lotesProductosDTO.PesoTotal;
                    pesoDisponible = lotesProductosDTO.PesoTotal;
                }

                LotesProductosEntity entity = new()
                {
                    IdProducto = lotesProductosDTO.IdProducto,
                    IdProveedor = lotesProductosDTO.IdProveedor,
                    FechaEntrada = DateTime.Now,
                    FechaVencimiento = lotesProductosDTO.FechaVencimiento,
                    CantidadUnidades = lotesProductosDTO.CantidadUnidades,
                    PesoTotal = lotesProductosDTO.PesoTotal,
                    IdUnidadMedida = lotesProductosDTO.IdUnidadMedida,
                    PrecioUnitario = lotesProductosDTO.PrecioUnitario,
                    PrecioKilo = lotesProductosDTO.PrecioKilo,
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
                    Cantidad = lotesProductosDTO.PesoTotal,
                    TotalMovimiento = precioTotal,
                    IdUnidadMedida = lotesProductosDTO.IdUnidadMedida,
                    IdUsuario = lotesProductosDTO.IdUsuario,
                    Observacion = $"Ingreso de lote - {lotesProductosDTO.CantidadUnidades} unidades, {lotesProductosDTO.PesoTotal} kg",
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
                var diferenciaCantidad = lotesProductosDTO.CantidadUnidades - lote.CantidadUnidades;
                var diferenciaPeso = lotesProductosDTO.PesoTotal - lote.PesoTotal;

                var cantidadConsumida = lote.CantidadUnidades - lote.CantidadDisponible;
                var pesoConsumido = lote.PesoTotal - lote.PesoDisponible;

                if (lotesProductosDTO.CantidadUnidades < cantidadConsumida || lotesProductosDTO.PesoTotal < pesoConsumido)
                {
                    throw new InvalidOperationException(
                        "No se puede reducir el lote por debajo de lo ya consumido");
                }

                lote.IdProducto = lotesProductosDTO.IdProducto;
                lote.IdProveedor = lotesProductosDTO.IdProveedor;
                lote.FechaVencimiento = lotesProductosDTO.FechaVencimiento;
                lote.CantidadUnidades = lotesProductosDTO.CantidadUnidades;
                lote.PesoTotal = lotesProductosDTO.PesoTotal;
                lote.IdUnidadMedida = lotesProductosDTO.IdUnidadMedida;
                lote.PrecioUnitario = lotesProductosDTO.PrecioUnitario;
                lote.PrecioKilo = lotesProductosDTO.PrecioKilo;
                lote.PrecioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);
                lote.IdMarca = lotesProductosDTO.IdMarca;

                lote.CantidadDisponible += diferenciaCantidad;
                lote.PesoDisponible += diferenciaPeso;

                await _lotesProductosRepository.UpdateAsync(lote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        cantidadUnidades = lotesProductosDTO.CantidadUnidades,
                        diferenciaCantidad,
                        pesoTotal = lotesProductosDTO.PesoTotal,
                        diferenciaPeso
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
                    Observacion = $"Eliminación de lote - {existente.CantidadDisponible} kg",
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
                Estado = l.Estado
            }).ToList();
        }

        private static decimal CalculoPrecioTotal(int cantidadUnidades, decimal precioUnitario)
        {
            var precioTotal = cantidadUnidades * precioUnitario;
            return precioTotal;
        }
    }
}
