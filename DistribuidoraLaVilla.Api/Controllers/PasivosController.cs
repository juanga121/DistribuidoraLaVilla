using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasivosController(PasivosService pasivosService) : Controller
    {
        private readonly PasivosService _pasivosService = pasivosService;

        [HttpPost]
        [Route("CrearPasivo")]
        public async Task<IActionResult> CrearPasivo([FromBody] PasivosDTO pasivosDTO)
        {
            await _pasivosService.CrearPasivoAsync(pasivosDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerPasivos")]
        public async Task<IActionResult> ObtenerPasivos()
        {
            var pasivos = await _pasivosService.ObtenerPasivosAsync();
            return Ok(pasivos);
        }

        [HttpGet]
        [Route("ObtenerPasivoPorId/{id:int}")]
        public async Task<IActionResult> ObtenerPasivoPorId(int id)
        {
            var pasivo = await _pasivosService.ObtenerPasivoPorIdAsync(id);
            return Ok(pasivo);
        }

        [HttpPut]
        [Route("ActualizarPasivo/{id:int}")]
        public async Task<IActionResult> ActualizarPasivo(int id, [FromBody] PasivosDTO pasivosDTO)
        {
            await _pasivosService.ActualizarPasivoAsync(id, pasivosDTO, GetUserId());
            return Ok();
        }

        [HttpDelete]
        [Route("EliminarPasivo/{id:int}")]
        public async Task<IActionResult> EliminarPasivo(int id)
        {
            await _pasivosService.EliminarPasivoAsync(id);
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
