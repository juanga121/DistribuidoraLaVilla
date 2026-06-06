using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Domain.DTOS;
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
        IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository = lotesProductosRepository;
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosProductosRepository = movimientosProductosRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearLoteProductoAsync(LotesProductosDTO lotesProductosDTO)
        {
            // Validación
            var validator = new LotesProductosDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesProductosDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var precioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);

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
                CantidadInicial = lotesProductosDTO.PesoTotal,
                CantidadDisponible = lotesProductosDTO.PesoTotal,
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

            // Auto-generar movimiento de entrada
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
        }

        public async Task<List<LotesProductosEntity>> ObtenerLotesProductosAsync()
        {
            return await _lotesProductosRepository.GetAllAsync();
        }

        public async Task<LotesProductosEntity> ObtenerLoteProductoPorIdAsync(int id)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(id);
            return lote ?? throw new Exception("El lote de producto no existe");
        }

        public async Task ActualizarEstadoLoteProducto(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (lote != null)
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

                // Si se da de baja (estado = 0), auto-generar movimiento de vencimiento
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
            }
            else
            {
                throw new Exception("El lote de producto no existe");
            }
        }

        public async Task ActualizarLoteProducto(int id, LotesProductosDTO lotesProductosDTO)
        {
            // Validación
            var validator = new LotesProductosDTOValidator();
            var validationResult = await validator.ValidateAsync(lotesProductosDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var lote = await _lotesProductosRepository.FindByIdAsync(id);
            if (lote != null)
            {
                // Calculamos la diferencia para ajustar CantidadDisponible
                var diferenciaPeso = lotesProductosDTO.PesoTotal - lote.PesoTotal;

                lote.IdProducto = lotesProductosDTO.IdProducto;
                lote.IdProveedor = lotesProductosDTO.IdProveedor;
                lote.FechaEntrada = DateTime.Now;
                lote.FechaVencimiento = lotesProductosDTO.FechaVencimiento;
                lote.CantidadUnidades = lotesProductosDTO.CantidadUnidades;
                lote.PesoTotal = lotesProductosDTO.PesoTotal;
                lote.IdUnidadMedida = lotesProductosDTO.IdUnidadMedida;
                lote.PrecioUnitario = lotesProductosDTO.PrecioUnitario;
                lote.PrecioKilo = lotesProductosDTO.PrecioKilo;
                lote.PrecioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);
                lote.IdMarca = lotesProductosDTO.IdMarca;
                lote.CantidadInicial = lotesProductosDTO.PesoTotal;
                lote.CantidadDisponible += diferenciaPeso;
                await _lotesProductosRepository.UpdateAsync(lote);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        cantidadUnidades = lotesProductosDTO.CantidadUnidades,
                        pesoTotal = lotesProductosDTO.PesoTotal,
                        diferenciaPeso
                    });
                    await _auditoriaService.RegistrarAsync("StockProducto", id.ToString(), "Modificar", detalle, lotesProductosDTO.IdUsuario);
                }
                catch { /* fire-and-forget */ }
            }
        }

        public async Task<List<LotesProductosEntity>> ObtenerLotesProductosDisponiblesAsync()
        {
            var lotes = await _lotesProductosRepository.GetAllAsync();
            return [.. lotes.Where(l => l.Estado == 1)];
        }

        public async Task EliminarLoteProductoAsync(int idLote, Guid idUsuario)
        {
            var existente = await _lotesProductosRepository.FindByIdAsync(idLote);
            if (existente != null)
            {
                // Auto-generar movimiento de ajuste antes de eliminar
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
            }
            else
            {
                throw new Exception("El lote de producto no existe");
            }
        }

        private static decimal CalculoPrecioTotal(int cantidadUnidades, decimal precioUnitario)
        {
            var precioTotal = cantidadUnidades * precioUnitario;
            return precioTotal;
        }
    }
}
