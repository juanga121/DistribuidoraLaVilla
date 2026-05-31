using System.Security.Claims;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesMateriaPrimaController(LotesMateriaPrimaService lotesMateriaPrimaService) : Controller
    {
        private readonly LotesMateriaPrimaService _lotesMateriaPrimaService = lotesMateriaPrimaService;

        [HttpPost]
        [Route("CrearLoteMateriaPrima")]
        public async Task<IActionResult> CrearLoteMateriaPrima([FromBody] LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            try
            {
                await _lotesMateriaPrimaService.CrearLoteMateriaPrimaAsync(lotesMateriaPrimaDTO);
                return Ok(new { mensaje = "Lote de materia prima creado exitosamente" });
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
        [Route("ObtenerLotesMateriaPrima")]
        public async Task<IActionResult> ObtenerLotesMateriaPrima()
        {
            var lotes = await _lotesMateriaPrimaService.ObtenerLotesMateriaPrimaAsync();
            return Ok(lotes);
        }

        [HttpGet]
        [Route("ObtenerLoteMateriaPrimaPorId/{id}")]
        public async Task<IActionResult> ObtenerLoteMateriaPrimaPorId(int id)
        {
            var lote = await _lotesMateriaPrimaService.ObtenerLoteMateriaPrimaPorIdAsync(id);
            return Ok(lote);
        }

        [HttpPut]
        [Route("ActualizarEstadoLoteMateriaPrima")]
        public async Task<IActionResult> ActualizarEstadoLoteMateriaPrima([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            await _lotesMateriaPrimaService.ActualizarEstadoLoteMateriaPrima(actualizarEstadoDTO);
            return Ok();
        }

        [HttpPut]
        [Route("ActualizarLoteMateriaPrima/{idLoteMateriaPrima}")]
        public async Task<IActionResult> ActualizarLoteMateriaPrima(int idLoteMateriaPrima, [FromBody] LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            try
            {
                await _lotesMateriaPrimaService.ActualizarLoteMateriaPrima(idLoteMateriaPrima, lotesMateriaPrimaDTO);
                return Ok(new { mensaje = "Lote de materia prima actualizado exitosamente" });
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
        [Route("ObtenerLotesMateriaPrimaDisponibles")]
        public async Task<IActionResult> ObtenerLotesMateriaPrimaDisponibles()
        {
            var lotes = await _lotesMateriaPrimaService.ObtenerLotesMateriaPrimaDisponiblesAsync();
            return Ok(lotes);
        }

        [HttpDelete]
        [Route("EliminarLoteMateriaPrima/{idLote}")]
        public async Task<IActionResult> EliminarLote(int idLote)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            await _lotesMateriaPrimaService.EliminarLoteMateriaPrimaAsync(idLote, Guid.Parse(userIdClaim));
            return Ok();
        }
    }
}
