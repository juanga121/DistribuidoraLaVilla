using Microsoft.AspNetCore.Mvc;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarcaController(MarcasService marcasService) : Controller
    {
        private readonly MarcasService _marcasService = marcasService;
        [HttpPost]
        [Route("CrearMarca")]
        public async Task<IActionResult> CrearMarca([FromBody] MarcasDTO marcasDTO)
        {
            try
            {
                await _marcasService.CrearMarcaAsync(marcasDTO, GetUserId());
                return Ok(new { success = true, message = "Marca creada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("ObtenerMarcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            try
            {
                var marcas = await _marcasService.ObtenerMarcasAsync();
                return Ok(new { success = true, message = "Marcas obtenidas correctamente", data = marcas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("ObtenerMarcaPorId/{id:int}")]
        public async Task<IActionResult> ObtenerMarcaPorId(int id)
        {
            try
            {
                var marca = await _marcasService.ObtenerMarcaPorIdAsync(id);
                return Ok(new { success = true, message = "Marca obtenida correctamente", data = marca });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
        [HttpPut]
        [Route("ActualizarEstadoMarca")]
        public async Task<IActionResult> ActualizarEstadoMarca([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            try
            {
                await _marcasService.ActualizarEstadoMarca(actualizarEstadoDTO, GetUserId());
                return Ok(new { success = true, message = "Estado de la marca actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
        [HttpPut]
        [Route("ActualizarMarca/{idMarca}")]
        public async Task<IActionResult> ActualizarMarca(int idMarca, [FromBody] MarcasDTO marcasDTO)
        {
            try
            {
                await _marcasService.ActualizarMarca(idMarca, marcasDTO, GetUserId());
                return Ok(new { success = true, message = "Marca actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMarcasDisponibles")]
        public async Task<IActionResult> ObtenerMarcasDisponibles()
        {
            try
            {
                var resultado = await _marcasService.ObtenerMarcasDisponibles();
                return Ok(new { success = true, message = "Marcas disponibles obtenidas correctamente", data = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarMarca/{idMarca}")]
        public async Task<IActionResult> EliminarMarca(int idMarca)
        {
            try
            {
                await _marcasService.EliminarMarcaAsync(idMarca, GetUserId());
                return Ok(new { success = true, message = "Marca eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(nameIdentifier) && Guid.TryParse(nameIdentifier, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
