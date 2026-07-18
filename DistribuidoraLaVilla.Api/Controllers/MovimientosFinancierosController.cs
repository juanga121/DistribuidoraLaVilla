using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosFinancierosController(MovimientosFinancierosService movimientosService) : Controller
    {
        private readonly MovimientosFinancierosService _movimientosService = movimientosService;

        [HttpPost]
        [Route("CrearMovimiento")]
        public async Task<IActionResult> CrearMovimiento([FromBody] CrearMovimientoFinancieroDTO dto)
        {
            await _movimientosService.CrearMovimientoAsync(dto, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerMovimientos")]
        public async Task<IActionResult> ObtenerMovimientos([FromQuery] int? estado = null)
        {
            var result = await _movimientosService.ObtenerMovimientosAsync(estado);
            return Ok(result);
        }

        [HttpGet]
        [Route("ObtenerMovimientoPorId/{id:int}")]
        public async Task<IActionResult> ObtenerMovimientoPorId(int id)
        {
            var result = await _movimientosService.ObtenerMovimientoPorIdAsync(id);
            return Ok(result);
        }

        [HttpPut]
        [Route("ActualizarMovimiento/{id:int}")]
        public async Task<IActionResult> ActualizarMovimiento(int id, [FromBody] CrearMovimientoFinancieroDTO dto)
        {
            await _movimientosService.ActualizarMovimientoAsync(id, dto, GetUserId());
            return Ok();
        }

        [HttpDelete]
        [Route("EliminarMovimiento/{id:int}")]
        public async Task<IActionResult> EliminarMovimiento(int id)
        {
            await _movimientosService.EliminarMovimientoAsync(id);
            return Ok();
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
