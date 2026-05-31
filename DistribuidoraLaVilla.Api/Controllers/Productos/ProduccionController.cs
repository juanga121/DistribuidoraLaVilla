using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProduccionController : ControllerBase
    {
        private readonly ProduccionService _produccionService;

        public ProduccionController(ProduccionService produccionService)
        {
            _produccionService = produccionService;
        }

        /// <summary>
        /// Procesa una orden de producción completa (consume materia prima y genera producto terminado)
        /// </summary>
        /// <param name="solicitud">Datos de la producción</param>
        /// <returns>Resultado detallado con movimientos y lote generado</returns>
        [HttpPost("ProcesarProduccion")]
        public async Task<ActionResult<ResultadoProduccionDTO>> ProcesarProduccion([FromBody] SolicitudProduccionDTO solicitud)
        {
            try
            {
                var resultado = await _produccionService.ProcesarProduccionAsync(solicitud);

                if (!resultado.Exitoso)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = resultado.Mensaje,
                        data = resultado
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = resultado.Mensaje,
                    data = resultado
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error de validación",
                    errors = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene todas las órdenes de producción
        /// </summary>
        /// <returns>Lista de órdenes</returns>
        [HttpGet("ObtenerOrdenes")]
        public async Task<ActionResult<List<OrdenProduccionEntity>>> ObtenerOrdenes()
        {
            var ordenes = await _produccionService.ObtenerOrdenesProduccionAsync();
            return Ok(new
            {
                success = true,
                message = "Órdenes de producción obtenidas correctamente",
                data = ordenes
            });
        }

        /// <summary>
        /// Obtiene una orden de producción por ID
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Detalle de la orden</returns>
        [HttpGet("ObtenerOrdenPorId/{id}")]
        public async Task<ActionResult<OrdenProduccionEntity>> ObtenerOrdenPorId(int id)
        {
            var orden = await _produccionService.ObtenerOrdenPorIdAsync(id);
            if (orden == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró la orden de producción con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Orden obtenida correctamente",
                data = orden
            });
        }

        /// <summary>
        /// Obtiene órdenes por estado
        /// </summary>
        /// <param name="estado">Estado: 0=Cancelada, 1=Pendiente, 2=Completada</param>
        /// <returns>Lista de órdenes filtradas</returns>
        [HttpGet("ObtenerOrdenesPorEstado/{estado}")]
        public ActionResult<List<OrdenProduccionEntity>> ObtenerOrdenesPorEstado(int estado)
        {
            var ordenes = _produccionService.ObtenerOrdenesPorEstado(estado);
            return Ok(new
            {
                success = true,
                message = $"Órdenes con estado {estado} obtenidas correctamente",
                data = ordenes
            });
        }

        /// <summary>
        /// Cancela una orden de producción pendiente
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="idUsuario">ID del usuario que cancela</param>
        /// <returns>Confirmación de cancelación</returns>
        [HttpPut("CancelarOrden")]
        public async Task<ActionResult> CancelarOrden([FromQuery] int id, [FromQuery] Guid idUsuario)
        {
            var resultado = await _produccionService.CancelarOrdenAsync(id, idUsuario);
            if (!resultado)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se pudo cancelar la orden. Verifique que exista y esté en estado Pendiente"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Orden de producción cancelada correctamente"
            });
        }
    }
}
