using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionesService _notificacionesService;

        public NotificacionesController(NotificacionesService notificacionesService)
        {
            _notificacionesService = notificacionesService;
        }

        /// <summary>
        /// Obtiene el resumen de alertas: CxC vencidas/próximas, CxP
        /// vencidas/próximas, y stock de productos/materia prima por vencer.
        /// </summary>
        /// <param name="dias">Días de anticipación (default: 30)</param>
        [HttpGet("resumen")]
        public async Task<ActionResult<NotificacionesResumenDTO>> ObtenerResumen([FromQuery] int dias = 30)
        {
            var resumen = await _notificacionesService.ObtenerResumenAsync(dias);
            return Ok(new
            {
                success = true,
                message = $"Alertas obtenidas con {dias} días de anticipación",
                data = resumen
            });
        }
    }
}