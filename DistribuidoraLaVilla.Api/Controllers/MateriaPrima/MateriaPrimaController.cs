using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            await _materiaPrimaService.CrearMateriaPrimaAsync(materiaPrimaDTO, GetUserId());
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
            try
            {
                var result = await _materiaPrimaService.ObtnerMateriPrimaPorId(idMateriaPrima);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("no se encontro", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
            }
        }

        [HttpPut]
        [Route("ActualizarEstadoMateriaPrima")]
        public async Task<IActionResult> ActualizarEstadoMateriaPrima([FromBody] MateriaPrimaActualizarEstadoDTO materiaPrimaActualizarEstadoDTO)
        {
            try
            {
                await _materiaPrimaService.ActualizarEstadoMateriaPrima(materiaPrimaActualizarEstadoDTO, GetUserId());
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
            }
        }

        [HttpPut]
        [Route("ActualizarMateriaPrima/{idMateriaPrima}")]
        public async Task<IActionResult> ActualizarMateriaPrima(int idMateriaPrima, [FromBody] MateriaPrimaDTO materiaPrimaDTO)
        {
            try
            {
                await _materiaPrimaService.ActualizarMateriaPrima(idMateriaPrima, materiaPrimaDTO, GetUserId());
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
            }
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
            try
            {
                await _materiaPrimaService.EliminarMateriaPrimaAsync(idMateriaPrima, GetUserId());
                return Ok(new { mensaje = "Materia prima eliminada correctamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
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
