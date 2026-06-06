using DistribuidoraLaVilla.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController(IAuditoriaService auditoriaService) : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        [HttpGet]
        public async Task<IActionResult> Obtener(
            [FromQuery] string? entidad = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int? idUsuario = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                var registros = await _auditoriaService.ObtenerAsync(entidad, desde, hasta, idUsuario, limit);
                return Ok(new
                {
                    registros,
                    total = registros.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener auditoría: {ex.Message}" });
            }
        }
    }
}
