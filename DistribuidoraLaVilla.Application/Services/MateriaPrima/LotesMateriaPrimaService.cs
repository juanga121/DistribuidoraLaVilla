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
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class LotesMateriaPrimaService(
        IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository,
        IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosMateriaPrimaRepository,
        IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository = lotesMateriaPrimaRepository;
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosMateriaPrimaRepository = movimientosMateriaPrimaRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearLoteMateriaPrimaAsync(LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            // Validación
            var validator = new LotesMateriaPrimaDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var CostoTotal = CalculoCostoTotal(lotesMateriaPrimaDTO.Cantidad, lotesMateriaPrimaDTO.CostoUnitario);

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

            // Auto-generar movimiento de entrada
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
        }

        public async Task<List<LotesMateriaPrimaEntity>> ObtenerLotesMateriaPrimaAsync()
        {
            return await _lotesMateriaPrimaRepository.GetAllAsync();
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
                lote.Estado = actualizarEstadoDTO.EstadoNuevo;
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

                // Si se da de baja (estado = 0), auto-generar movimiento de vencimiento
                if (actualizarEstadoDTO.EstadoNuevo == 0 && estadoAnterior != 0)
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
            // Validación
            var validator = new LotesMateriaPrimaDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesMateriaPrimaDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(id);
            if (lote != null)
            {
                // Calculamos la diferencia para ajustar CantidadDisponible
                var diferenciaCantidad = lotesMateriaPrimaDTO.Cantidad - lote.Cantidad;

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
                // Auto-generar movimiento de ajuste antes de eliminar
                var movimiento = new MovimientosMateriaPrimaEntity
                {
                    IdLoteMateria = existente.Id,
                    IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Ajuste,
                    Fecha = DateTime.Now,
                    Cantidad = existente.CantidadDisponible,
                    IdUnidadMedida = existente.IdUnidadMedida,
                    IdUsuario = idUsuario,
                    Observacion = $"Eliminación de lote - {existente.CantidadDisponible} unidades"
                };
                await _movimientosMateriaPrimaRepository.CreateAsync(movimiento);

                await _lotesMateriaPrimaRepository.DeleteAsync(idLote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        cantidadDisponible = existente.CantidadDisponible
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
