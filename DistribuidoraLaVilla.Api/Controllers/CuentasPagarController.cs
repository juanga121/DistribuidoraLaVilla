using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasPagarController(ICuentasPagarService cuentasPagarService) : Controller
    {
        private readonly ICuentasPagarService _cuentasPagarService = cuentasPagarService;

        [HttpPost]
        [Route("Crear")]
        public async Task<IActionResult> Crear([FromBody] CrearCxPDTO dto)
        {
            try
            {
                var result = await _cuentasPagarService.CrearAsync(dto, GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerTodos")]
        public async Task<IActionResult> ObtenerTodos([FromQuery] int? estado = null)
        {
            try
            {
                var result = await _cuentasPagarService.ObtenerTodosAsync(estado);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerPorId/{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var result = await _cuentasPagarService.ObtenerPorIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerPagos/{id:int}")]
        public async Task<IActionResult> ObtenerPagos(int id)
        {
            try
            {
                var result = await _cuentasPagarService.ObtenerPagosAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("Actualizar/{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CrearCxPDTO dto)
        {
            try
            {
                var result = await _cuentasPagarService.ActualizarAsync(id, dto, GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _cuentasPagarService.EliminarAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("RegistrarPago/{id:int}")]
        public async Task<IActionResult> RegistrarPago(int id, [FromBody] RegistrarPagoCxPDTO dto)
        {
            try
            {
                var result = await _cuentasPagarService.RegistrarPagoAsync(id, dto, GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
