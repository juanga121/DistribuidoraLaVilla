using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class LotesMateriaPrimaService(
        IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository,
        IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosMateriaPrimaRepository,
        IGenericRepository<MarcasEntity, int> marcasRepository,
        IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
        IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
        IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
        IAuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository = lotesMateriaPrimaRepository;
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosMateriaPrimaRepository = movimientosMateriaPrimaRepository;
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository = materiaPrimaRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository = unidadMedidaRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task CrearLoteMateriaPrimaAsync(LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            var validator = new LotesMateriaPrimaDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var CostoTotal = CalculoCostoTotal(lotesMateriaPrimaDTO.Cantidad, lotesMateriaPrimaDTO.CostoUnitario);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                LotesMateriaPrimaEntity entity = new()
                {
                    IdMarca = lotesMateriaPrimaDTO.IdMarca,
                    IdMateria = lotesMateriaPrimaDTO.IdMateria,
                    IdProveedor = lotesMateriaPrimaDTO.IdProveedor,
                    FechaEntrada = DateTime.Now,
                    FechaVencimiento = lotesMateriaPrimaDTO.FechaVencimiento,
                    Cantidad = lotesMateriaPrimaDTO.Cantidad,
                    IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida,
                    CostoUnitario = lotesMateriaPrimaDTO.CostoUnitario,
                    CostoTotal = CostoTotal,
                    CantidadInicial = lotesMateriaPrimaDTO.Cantidad,
                    CantidadDisponible = lotesMateriaPrimaDTO.Cantidad,
                    Estado = 1
                };
                await _lotesMateriaPrimaRepository.CreateAsync(entity);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        idMateria = entity.IdMateria,
                        cantidad = entity.Cantidad,
                        costoUnitario = entity.CostoUnitario,
                        costoTotal = CostoTotal,
                        proveedor = entity.IdProveedor
                    });
                    await _auditoriaService.RegistrarAsync("StockMP", entity.Id.ToString(), "CrearLote", detalle, lotesMateriaPrimaDTO.IdUsuario);
                }
                catch { /* fire-and-forget */ }

                var movimiento = new MovimientosMateriaPrimaEntity
                {
                    IdLoteMateria = entity.Id,
                    IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Entrada,
                    Fecha = DateTime.Now,
                    Cantidad = lotesMateriaPrimaDTO.Cantidad,
                    IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida,
                    IdUsuario = lotesMateriaPrimaDTO.IdUsuario,
                    Observacion = $"Ingreso de lote - {lotesMateriaPrimaDTO.Cantidad} unidades"
                };
                await _movimientosMateriaPrimaRepository.CreateAsync(movimiento);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            finally
            {
                await _unitOfWork.DisposeAsync();
            }
        }

        public async Task<List<LotesMateriaPrimaEntity>> ObtenerLotesMateriaPrimaAsync()
        {
            return await _lotesMateriaPrimaRepository.GetAllAsync();
        }

        public async Task<List<LoteMateriaPrimaDetalleDTO>> ObtenerLotesMateriaPrimaDetalleAsync()
        {
            var lotes = (await _lotesMateriaPrimaRepository.GetAllAsync()).Where(l => l.Estado == 1).ToList();
            var marcas = (await _marcasRepository.GetAllAsync()).ToDictionary(m => m.IdMarca);
            var materias = (await _materiaPrimaRepository.GetAllAsync()).ToDictionary(m => m.Id);
            var proveedores = (await _proveedoresRepository.GetAllAsync()).ToDictionary(p => p.IdProveedor);
            var unidades = (await _unidadMedidaRepository.GetAllAsync()).ToDictionary(u => u.Id);

            return lotes.Select(l => new LoteMateriaPrimaDetalleDTO
            {
                Id = l.Id,
                IdMarca = l.IdMarca,
                NombreMarca = marcas.ContainsKey(l.IdMarca) ? marcas[l.IdMarca].Nombre : "N/A",
                IdMateria = l.IdMateria,
                NombreMateria = materias.ContainsKey(l.IdMateria) ? materias[l.IdMateria].Nombre : "N/A",
                IdProveedor = l.IdProveedor,
                NombreProveedor = proveedores.ContainsKey(l.IdProveedor) ? proveedores[l.IdProveedor].Nombre : "N/A",
                FechaEntrada = l.FechaEntrada,
                FechaVencimiento = l.FechaVencimiento,
                Cantidad = l.Cantidad,
                IdUnidadMedida = l.IdUnidadMedida,
                NombreUnidadMedida = unidades.ContainsKey(l.IdUnidadMedida) ? unidades[l.IdUnidadMedida].Nombre : "N/A",
                SimboloUnidadMedida = unidades.ContainsKey(l.IdUnidadMedida) ? unidades[l.IdUnidadMedida].Abreviatura : "N/A",
                CostoUnitario = l.CostoUnitario,
                CostoTotal = l.CostoTotal,
                CantidadInicial = l.CantidadInicial,
                CantidadDisponible = l.CantidadDisponible,
                Estado = l.Estado,
                NombreEstado = l.Estado switch
                {
                    1 => "Disponible",
                    2 => "En uso",
                    3 => "Agotado",
                    4 => "Vencido",
                    5 => "Bloqueado",
                    6 => "Devuelto",
                    7 => "En cuarentena",
                    _ => "Desconocido"
                },
                DiasParaVencimiento = (int)(l.FechaVencimiento - DateTime.Now).TotalDays
            }).ToList();
        }

        public async Task<LotesMateriaPrimaEntity> ObtenerLoteMateriaPrimaPorIdAsync(int id)
        {
            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(id);
            return lote ?? throw new Exception("El lote de materia prima no existe");
        }

        public async Task ActualizarEstadoLoteMateriaPrima(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (lote != null)
            {
                var estadoAnterior = lote.Estado;
                var estadoNuevo = actualizarEstadoDTO.EstadoNuevo == 0 ? 5 : actualizarEstadoDTO.EstadoNuevo;
                lote.Estado = estadoNuevo;
                await _lotesMateriaPrimaRepository.UpdateAsync(lote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        estadoAnterior,
                        estadoNuevo = lote.Estado
                    });
                    await _auditoriaService.RegistrarAsync("StockMP", lote.Id.ToString(), "CambioEstado", detalle, actualizarEstadoDTO.IdUsuario ?? Guid.Empty);
                }
                catch { /* fire-and-forget */ }

                if (estadoNuevo == 4 && estadoAnterior != 4)
                {
                    var movimiento = new MovimientosMateriaPrimaEntity
                    {
                        IdLoteMateria = lote.Id,
                        IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Vencimiento,
                        Fecha = DateTime.Now,
                        Cantidad = lote.CantidadDisponible,
                        IdUnidadMedida = lote.IdUnidadMedida,
                        IdUsuario = actualizarEstadoDTO.IdUsuario ?? Guid.Empty,
                        Observacion = $"Baja de lote por vencimiento - {lote.CantidadDisponible} unidades"
                    };
                    await _movimientosMateriaPrimaRepository.CreateAsync(movimiento);
                }
            }
            else
            {
                throw new Exception("El lote de materia prima no existe");
            }
        }

        public async Task ActualizarLoteMateriaPrima(int id, LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            var validator = new LotesMateriaPrimaDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(id);
                if (lote != null)
                {
                    var cantidadAnterior = lote.Cantidad;

                    var diferenciaCantidad = lotesMateriaPrimaDTO.Cantidad - cantidadAnterior;

                    lote.IdMarca = lotesMateriaPrimaDTO.IdMarca;
                    lote.IdMateria = lotesMateriaPrimaDTO.IdMateria;
                    lote.IdProveedor = lotesMateriaPrimaDTO.IdProveedor;
                    lote.FechaEntrada = DateTime.Now;
                    lote.FechaVencimiento = lotesMateriaPrimaDTO.FechaVencimiento;
                    lote.Cantidad = lotesMateriaPrimaDTO.Cantidad;
                    lote.IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida;
                    lote.CostoUnitario = lotesMateriaPrimaDTO.CostoUnitario;
                    lote.CostoTotal = CalculoCostoTotal(lotesMateriaPrimaDTO.Cantidad, lotesMateriaPrimaDTO.CostoUnitario);
                    lote.CantidadInicial = lotesMateriaPrimaDTO.Cantidad;
                    lote.CantidadDisponible += diferenciaCantidad;
                    await _lotesMateriaPrimaRepository.UpdateAsync(lote);

                    if (diferenciaCantidad != 0)
                    {
                        var movimientoAjuste = new MovimientosMateriaPrimaEntity
                        {
                            IdLoteMateria = lote.Id,
                            IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Ajuste,
                            Fecha = DateTime.Now,
                            Cantidad = diferenciaCantidad,
                            IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida,
                            IdUsuario = lotesMateriaPrimaDTO.IdUsuario,
                            Observacion = $"Ajuste por actualización de lote: {cantidadAnterior} → {lotesMateriaPrimaDTO.Cantidad} (diferencia: {diferenciaCantidad})"
                        };
                        await _movimientosMateriaPrimaRepository.CreateAsync(movimientoAjuste);
                    }

                    try
                    {
                        var detalle = JsonSerializer.Serialize(new
                        {
                            cantidad = lotesMateriaPrimaDTO.Cantidad,
                            costoUnitario = lotesMateriaPrimaDTO.CostoUnitario,
                            diferenciaCantidad
                        });
                        await _auditoriaService.RegistrarAsync("StockMP", id.ToString(), "Modificar", detalle, lotesMateriaPrimaDTO.IdUsuario);
                    }
                    catch { /* fire-and-forget */ }
                }
                else
                {
                    throw new Exception("El lote de materia prima no existe");
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            finally
            {
                await _unitOfWork.DisposeAsync();
            }
        }

        public async Task<List<LotesMateriaPrimaEntity>> ObtenerLotesMateriaPrimaDisponiblesAsync()
        {
            var lotes = await _lotesMateriaPrimaRepository.GetAllAsync();
            return [.. lotes.Where(l => l.Estado == 1)];
        }

        public async Task EliminarLoteMateriaPrimaAsync(int idLote, Guid idUsuario)
        {
            var existente = await _lotesMateriaPrimaRepository.FindByIdAsync(idLote);
            if (existente != null)
            {
                existente.Estado = 5;
                await _lotesMateriaPrimaRepository.UpdateAsync(existente);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        estadoNuevo = 5
                    });
                    await _auditoriaService.RegistrarAsync("StockMP", idLote.ToString(), "Eliminar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El lote de materia prima no existe");
            }
        }

        private static decimal CalculoCostoTotal(decimal cantidad, decimal costoUnitario)
        {
            var costoTotal = cantidad * costoUnitario;
            return costoTotal;
        }
    }
}
