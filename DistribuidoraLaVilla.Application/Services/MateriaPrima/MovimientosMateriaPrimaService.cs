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
    public class MovimientosMateriaPrimaService(
        IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosRepository,
        IGenericRepository<LotesMateriaPrimaEntity, int> lotesRepository,
        IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosRepository = movimientosRepository;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesRepository = lotesRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        /// <summary>
        /// Crea un nuevo movimiento de materia prima y actualiza el stock del lote
        /// </summary>
        public async Task<MovimientoMateriaPrimaResponseDTO> CrearMovimientoAsync(MovimientoMateriaPrimaDTO movimientoDTO)
        {
            // 1. VALIDACIÓN con FluentValidation
            var validator = new MovimientoMateriaPrimaDTOValidator();
            var validationResult = await validator.ValidateAsync(movimientoDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. OBTENER el lote de materia prima
            var lote = await _lotesRepository.FindByIdAsync(movimientoDTO.IdLoteMateria)
                ?? throw new Exception($"No se encontró el lote de materia prima con ID {movimientoDTO.IdLoteMateria}");

            // 3. GUARDAR stock anterior
            var stockAnterior = lote.CantidadDisponible;

            // 4. CALCULAR nuevo stock según tipo de movimiento
            var nuevoStock = CalcularNuevoStock(lote.CantidadDisponible, movimientoDTO.Cantidad, movimientoDTO.IdTipoMovimiento);

            // 5. VALIDAR que el stock no sea negativo
            if (nuevoStock < 0)
            {
                throw new Exception($"Stock insuficiente. Stock disponible: {lote.CantidadDisponible}, " +
                                  $"Cantidad solicitada: {movimientoDTO.Cantidad}. " +
                                  $"El movimiento resultaría en un stock negativo.");
            }

            // 6. ACTUALIZAR stock del lote
            lote.CantidadDisponible = nuevoStock;
            await _lotesRepository.UpdateAsync(lote);

            // 7. CREAR registro de movimiento
            MovimientosMateriaPrimaEntity movimiento = new()
            {
                IdLoteMateria = movimientoDTO.IdLoteMateria,
                IdTipoMovimiento = movimientoDTO.IdTipoMovimiento,
                Fecha = DateTime.Now,
                Cantidad = movimientoDTO.Cantidad,
                IdUnidadMedida = movimientoDTO.IdUnidadMedida,
                IdUsuario = movimientoDTO.IdUsuario,
                Observacion = movimientoDTO.Observacion
            };

            await _movimientosRepository.CreateAsync(movimiento);

            try
            {
                var detalle = JsonSerializer.Serialize(new
                {
                    tipoMovimiento = ObtenerNombreTipoMovimiento(movimiento.IdTipoMovimiento),
                    cantidad = movimiento.Cantidad,
                    stockAnterior,
                    stockNuevo = nuevoStock,
                    observacion = movimiento.Observacion
                });
                await _auditoriaService.RegistrarAsync("MovimientoMP", movimiento.Id.ToString(), "Crear", detalle, movimiento.IdUsuario);
            }
            catch { /* fire-and-forget */ }

            // 8. RETORNAR respuesta con información del movimiento
            return new MovimientoMateriaPrimaResponseDTO
            {
                Id = movimiento.Id,
                IdLoteMateria = movimiento.IdLoteMateria,
                IdTipoMovimiento = movimiento.IdTipoMovimiento,
                TipoMovimientoNombre = ObtenerNombreTipoMovimiento(movimiento.IdTipoMovimiento),
                Fecha = movimiento.Fecha,
                Cantidad = movimiento.Cantidad,
                IdUnidadMedida = movimiento.IdUnidadMedida,
                IdUsuario = movimiento.IdUsuario,
                Observacion = movimiento.Observacion,
                StockAnterior = stockAnterior,
                StockNuevo = nuevoStock
            };
        }

        /// <summary>
        /// Obtiene todos los movimientos de materia prima
        /// </summary>
        public async Task<List<MovimientosMateriaPrimaEntity>> ObtenerMovimientosAsync()
        {
            return await _movimientosRepository.GetAllAsync();
        }

        /// <summary>
        /// Obtiene un movimiento específico por ID
        /// </summary>
        public async Task<MovimientosMateriaPrimaEntity> ObtenerMovimientoPorIdAsync(int id)
        {
            var movimiento = await _movimientosRepository.FindByIdAsync(id);
            return movimiento ?? throw new Exception($"No se encontró el movimiento con ID {id}");
        }

        /// <summary>
        /// Obtiene todos los movimientos de un lote específico
        /// </summary>
        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorLoteAsync(int idLote)
        {
            return _movimientosRepository.GetByFilter(m => m.IdLoteMateria == idLote);
        }

        /// <summary>
        /// Obtiene movimientos por tipo
        /// </summary>
        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorTipoAsync(int idTipoMovimiento)
        {
            return _movimientosRepository.GetByFilter(m => m.IdTipoMovimiento == idTipoMovimiento);
        }

        /// <summary>
        /// Obtiene movimientos por rango de fechas
        /// </summary>
        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return _movimientosRepository.GetByFilter(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
        }

        /// <summary>
        /// Calcula el nuevo stock según el tipo de movimiento
        /// </summary>
        private static decimal CalcularNuevoStock(decimal stockActual, decimal cantidad, int tipoMovimiento)
        {
            return tipoMovimiento switch
            {
                (int)TipoMovimientoMateriaPrima.Entrada => stockActual + cantidad,      // Suma
                (int)TipoMovimientoMateriaPrima.Consumo => stockActual - cantidad,      // Resta
                (int)TipoMovimientoMateriaPrima.Ajuste => cantidad,                     // Establece el valor exacto
                (int)TipoMovimientoMateriaPrima.Devolucion => stockActual + cantidad,   // Suma
                (int)TipoMovimientoMateriaPrima.Vencimiento => stockActual - cantidad,  // Resta
                _ => throw new Exception($"Tipo de movimiento inválido: {tipoMovimiento}")
            };
        }

        /// <summary>
        /// Obtiene el nombre del tipo de movimiento
        /// </summary>
        private static string ObtenerNombreTipoMovimiento(int tipoMovimiento)
        {
            return tipoMovimiento switch
            {
                (int)TipoMovimientoMateriaPrima.Entrada => "Entrada",
                (int)TipoMovimientoMateriaPrima.Consumo => "Consumo",
                (int)TipoMovimientoMateriaPrima.Ajuste => "Ajuste",
                (int)TipoMovimientoMateriaPrima.Devolucion => "Devolución",
                (int)TipoMovimientoMateriaPrima.Vencimiento => "Vencimiento",
                _ => "Desconocido"
            };
        }
    }
}
