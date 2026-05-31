using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecetasProductoController : ControllerBase
    {
        private readonly RecetaProductoService _recetaService;

        public RecetasProductoController(RecetaProductoService recetaService)
        {
            _recetaService = recetaService;
        }

        /// <summary>
        /// Crea una nueva receta de producto (ingrediente requerido)
        /// </summary>
        /// <param name="dto">Datos de la receta</param>
        /// <returns>Receta creada con nombres completos</returns>
        [HttpPost("CrearReceta")]
        public async Task<ActionResult<RecetaProductoResponseDTO>> CrearReceta([FromBody] RecetaProductoDTO dto)
        {
            try
            {
                var resultado = await _recetaService.CrearRecetaAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = $"Receta creada correctamente: {resultado.NombreProducto} requiere {resultado.CantidadRequerida} {resultado.SimboloUnidadMedida} de {resultado.NombreMateriaPrima}",
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
        /// Obtiene todas las recetas activas
        /// </summary>
        /// <returns>Lista de recetas con información completa</returns>
        [HttpGet("ObtenerRecetas")]
        public async Task<ActionResult<List<RecetaProductoResponseDTO>>> ObtenerRecetas()
        {
            var recetas = await _recetaService.ObtenerRecetasAsync();
            return Ok(new
            {
                success = true,
                message = "Recetas obtenidas correctamente",
                data = recetas
            });
        }

        /// <summary>
        /// Obtiene una receta específica por ID
        /// </summary>
        /// <param name="id">ID de la receta</param>
        /// <returns>Detalle de la receta</returns>
        [HttpGet("ObtenerRecetaPorId/{id}")]
        public async Task<ActionResult<RecetaProductoResponseDTO>> ObtenerRecetaPorId(int id)
        {
            var receta = await _recetaService.ObtenerRecetaPorIdAsync(id);
            if (receta == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró la receta con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Receta obtenida correctamente",
                data = receta
            });
        }

        /// <summary>
        /// Obtiene la receta completa de un producto (todas las materias primas requeridas)
        /// </summary>
        /// <param name="idProducto">ID del producto</param>
        /// <returns>Lista de materias primas necesarias para el producto</returns>
        [HttpGet("ObtenerRecetaPorProducto/{idProducto}")]
        public async Task<ActionResult<List<RecetaProductoResponseDTO>>> ObtenerRecetaPorProducto(int idProducto)
        {
            var recetas = await _recetaService.ObtenerRecetaPorProductoAsync(idProducto);
            return Ok(new
            {
                success = true,
                message = $"Receta del producto obtenida correctamente ({recetas.Count} ingredientes)",
                data = recetas
            });
        }

        /// <summary>
        /// Actualiza una receta existente
        /// </summary>
        /// <param name="id">ID de la receta</param>
        /// <param name="dto">Nuevos datos de la receta</param>
        /// <returns>Receta actualizada</returns>
        [HttpPut("ActualizarReceta/{id}")]
        public async Task<ActionResult<RecetaProductoResponseDTO>> ActualizarReceta(int id, [FromBody] RecetaProductoDTO dto)
        {
            try
            {
                var resultado = await _recetaService.ActualizarRecetaAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    message = "Receta actualizada correctamente",
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
        /// Actualiza el estado de una receta (activar/desactivar)
        /// </summary>
        /// <param name="id">ID de la receta</param>
        /// <param name="nuevoEstado">Nuevo estado (0=Inactivo, 1=Activo)</param>
        /// <returns>Confirmación de actualización</returns>
        [HttpPut("ActualizarEstadoReceta")]
        public async Task<ActionResult> ActualizarEstadoReceta([FromQuery] int id, [FromQuery] int nuevoEstado)
        {
            var resultado = await _recetaService.ActualizarEstadoRecetaAsync(id, nuevoEstado);
            if (!resultado)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró la receta con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Estado de la receta actualizado a {nuevoEstado}"
            });
        }

        /// <summary>
        /// Elimina (desactiva) una receta
        /// </summary>
        /// <param name="id">ID de la receta a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("EliminarReceta/{id}")]
        public async Task<ActionResult> EliminarReceta(int id)
        {
            var resultado = await _recetaService.EliminarRecetaAsync(id);
            if (!resultado)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró la receta con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Receta eliminada (desactivada) correctamente"
            });
        }
    }
}
