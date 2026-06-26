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
            await _marcasService.CrearMarcaAsync(marcasDTO, GetUserId());
            return Ok();
        }
        [HttpGet]
        [Route("ObtenerMarcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            var marcas = await _marcasService.ObtenerMarcasAsync();
            return Ok(marcas);
        }
        [HttpGet]
        [Route("ObtenerMarcaPorId/{id:int}")]
        public async Task<IActionResult> ObtenerMarcaPorId(int id)
        {
            var marca = await _marcasService.ObtenerMarcaPorIdAsync(id);
            return Ok(marca);
        }
        [HttpPut]
        [Route("ActualizarEstadoMarca")]
        public async Task<IActionResult> ActualizarEstadoMarca([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            await _marcasService.ActualizarEstadoMarca(actualizarEstadoDTO, GetUserId());
            return Ok();
        }
        [HttpPut]
        [Route("ActualizarMarca/{idMarca}")]
        public async Task<IActionResult> ActualizarMarca(int idMarca, [FromBody] MarcasDTO marcasDTO)
        {
            await _marcasService.ActualizarMarca(idMarca, marcasDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerMarcasDisponibles")]
        public async Task<IActionResult> ObtenerMarcasDisponibles()
        {
            var resultado = await _marcasService.ObtenerMarcasDisponibles();
            return Ok(resultado);
        }

        [HttpDelete]
        [Route("EliminarMarca/{idMarca}")]
        public async Task<IActionResult> EliminarMarca(int idMarca)
        {
            await _marcasService.EliminarMarcaAsync(idMarca, GetUserId());
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
