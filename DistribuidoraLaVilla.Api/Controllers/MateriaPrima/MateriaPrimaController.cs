using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    public class MateriaPrimaController(MateriaPrimaService materiaPrimaService) : Controller
    {
        private readonly MateriaPrimaService _materiaPrimaService = materiaPrimaService;

        [HttpPost]
        [Route("CrearMateriaPrima")]
        public async Task<IActionResult> CrearMateriaPrima([FromBody] MateriaPrimaDTO materiaPrimaDTO)
        {
            await _materiaPrimaService.CrearMateriaPrimaAsync(materiaPrimaDTO);
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerMateriasPrimas")]
        public async Task<IActionResult> ObtenerMateriasPrimas()
        {
            var result = await _materiaPrimaService.ObtenerMateriaPrimaAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("ObtenerMateriaPrimaPorId/{idMateriaPrima}")]
        public async Task<IActionResult> ObtenerMateriaPrimaPorId(int idMateriaPrima)
        {
            var result = await _materiaPrimaService.ObtnerMateriPrimaPorId(idMateriaPrima);
            return Ok(result);
        }

        [HttpPut]
        [Route("ActualizarEstadoMateriaPrima")]
        public async Task<IActionResult> ActualizarEstadoMateriaPrima([FromBody] MateriaPrimaActualizarEstadoDTO materiaPrimaActualizarEstadoDTO)
        {
            await _materiaPrimaService.ActualizarEstadoMateriaPrima(materiaPrimaActualizarEstadoDTO);
            return Ok();
        }

        [HttpPut]
        [Route("ActualizarMateriaPrima/{idMateriaPrima}")]
        public async Task<IActionResult> ActualizarMateriaPrima(int idMateriaPrima, [FromBody] MateriaPrimaDTO materiaPrimaDTO)
        {
            await _materiaPrimaService.ActualizarMateriaPrima(idMateriaPrima, materiaPrimaDTO);
            return Ok();
        }      

        [HttpGet]
        [Route("ObtenerMateriasPrimasDisponibles")]
        public async Task<IActionResult> ObtenerMateriasPrimasDisponibles()
        {
            var materiaPrima = await _materiaPrimaService.ObtenerMateriaPrimaDisponible();
            return Ok(materiaPrima);
        }

        [HttpDelete]
        [Route("EliminarMateriaPrima/{idMateriaPrima}")]
        public async Task<IActionResult> EliminarMateriaPrima(int idMateriaPrima)
        {
            await _materiaPrimaService.EliminarMateriaPrimaAsync(idMateriaPrima);
            return Ok();
        }
    }
}
