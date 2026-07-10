using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivosController(ActivosService activosService) : Controller
    {
        private readonly ActivosService _activosService = activosService;

        [HttpPost]
        [Route("CrearActivo")]
        public async Task<IActionResult> CrearActivo([FromBody] ActivosDTO activosDTO)
        {
            await _activosService.CrearActivoAsync(activosDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerActivos")]
        public async Task<IActionResult> ObtenerActivos()
        {
            var activos = await _activosService.ObtenerActivosAsync();
            return Ok(activos);
        }

        [HttpGet]
        [Route("ObtenerActivoPorId/{id:int}")]
        public async Task<IActionResult> ObtenerActivoPorId(int id)
        {
            var activo = await _activosService.ObtenerActivoPorIdAsync(id);
            return Ok(activo);
        }

        [HttpPut]
        [Route("ActualizarActivo/{id:int}")]
        public async Task<IActionResult> ActualizarActivo(int id, [FromBody] ActivosDTO activosDTO)
        {
            await _activosService.ActualizarActivoAsync(id, activosDTO, GetUserId());
            return Ok();
        }

        [HttpDelete]
        [Route("EliminarActivo/{id:int}")]
        public async Task<IActionResult> EliminarActivo(int id)
        {
            await _activosService.EliminarActivoAsync(id);
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
