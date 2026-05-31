using System.Security.Claims;
using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesProductosController(LotesProductosService lotesProductosService) : Controller
    {
        private readonly LotesProductosService _lotesProductosService = lotesProductosService;

        [HttpPost]
        [Route("CrearLoteProducto")]
        public async Task<IActionResult> CrearLoteProducto([FromBody] LotesProductosDTO lotesProductosDTO)
        {
            try
            {
                await _lotesProductosService.CrearLoteProductoAsync(lotesProductosDTO);
                return Ok(new { mensaje = "Lote de producto creado exitosamente" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errores = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear el lote", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerLotesProductos")]
        public async Task<IActionResult> ObtenerLotesProductos()
        {
            var lotes = await _lotesProductosService.ObtenerLotesProductosAsync();
            return Ok(lotes);
        }

        [HttpGet]
        [Route("ObtenerLoteProductoPorId/{id}")]
        public async Task<IActionResult> ObtenerLoteProductoPorId(int id)
        {
            var lote = await _lotesProductosService.ObtenerLoteProductoPorIdAsync(id);
            return Ok(lote);
        }

        [HttpPut]
        [Route("ActualizarEstadoLoteProducto")]
        public async Task<IActionResult> ActualizarEstadoLoteProducto([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            await _lotesProductosService.ActualizarEstadoLoteProducto(actualizarEstadoDTO);
            return Ok();
        }

        [HttpPut]
        [Route("ActualizarLoteProducto/{idLoteProducto}")]
        public async Task<IActionResult> ActualizarLoteProducto(int idLoteProducto, [FromBody] LotesProductosDTO lotesProductosDTO)
        {
            try
            {
                await _lotesProductosService.ActualizarLoteProducto(idLoteProducto, lotesProductosDTO);
                return Ok(new { mensaje = "Lote de producto actualizado exitosamente" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errores = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar el lote", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerLotesProductosDisponibles")]
        public async Task<IActionResult> ObtenerLotesProductosDisponibles()
        {
            var lotes = await _lotesProductosService.ObtenerLotesProductosDisponiblesAsync();
            return Ok(lotes);
        }

        [HttpDelete]
        [Route("EliminarLoteProducto/{idLote}")]
        public async Task<IActionResult> EliminarLoteProducto(int idLote)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            await _lotesProductosService.EliminarLoteProductoAsync(idLote, Guid.Parse(userIdClaim));
            return Ok();
        }
    }
}
