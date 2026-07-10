using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatrimonioController(PatrimonioService patrimonioService) : Controller
    {
        private readonly PatrimonioService _patrimonioService = patrimonioService;

        [HttpPost]
        [Route("CrearPatrimonio")]
        public async Task<IActionResult> CrearPatrimonio([FromBody] PatrimonioDTO patrimonioDTO)
        {
            await _patrimonioService.CrearPatrimonioAsync(patrimonioDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerPatrimonios")]
        public async Task<IActionResult> ObtenerPatrimonios()
        {
            var patrimonios = await _patrimonioService.ObtenerPatrimoniosAsync();
            return Ok(patrimonios);
        }

        [HttpGet]
        [Route("ObtenerPatrimonioPorId/{id:int}")]
        public async Task<IActionResult> ObtenerPatrimonioPorId(int id)
        {
            var patrimonio = await _patrimonioService.ObtenerPatrimonioPorIdAsync(id);
            return Ok(patrimonio);
        }

        [HttpPut]
        [Route("ActualizarPatrimonio/{id:int}")]
        public async Task<IActionResult> ActualizarPatrimonio(int id, [FromBody] PatrimonioDTO patrimonioDTO)
        {
            await _patrimonioService.ActualizarPatrimonioAsync(id, patrimonioDTO, GetUserId());
            return Ok();
        }

        [HttpDelete]
        [Route("EliminarPatrimonio/{id:int}")]
        public async Task<IActionResult> EliminarPatrimonio(int id)
        {
            await _patrimonioService.EliminarPatrimonioAsync(id);
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
