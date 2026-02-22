using Microsoft.AspNetCore.Mvc;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;

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
            await _marcasService.CrearMarcaAsync(marcasDTO);
            return Ok("Marca agregada con exito");
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
            await _marcasService.ActualizarEstadoMarca(actualizarEstadoDTO);
            return Ok("Estado de la marca actualizado con exito");
        }
        [HttpPut]
        [Route("ActualizarMarca/{idMarca}")]
        public async Task<IActionResult> ActualizarMarca(int idMarca, [FromBody] MarcasDTO marcasDTO)
        {
            await _marcasService.ActualizarMarca(idMarca, marcasDTO);
            return Ok("Marca actualizada con exito");
        }

        [HttpGet]
        [Route("ObtenerMarcasDisponibles")]
        public async Task<IActionResult> ObtenerMarcasDisponibles()
        {
            var resultado = await _marcasService.ObtenerMarcasDisponibles();
            return Ok(resultado);
        }
    }
}
