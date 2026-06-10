using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecibosCajaController : ControllerBase
    {
        private readonly IRecibosCajaService _recibosCajaService;

        public RecibosCajaController(IRecibosCajaService recibosCajaService)
        {
            _recibosCajaService = recibosCajaService;
        }

        /// <summary>
        /// Retorna lista de recibos con filtros opcionales.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ReciboEntity>>> GetAll(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? metodoPago,
            [FromQuery] string? search)
        {
            try
            {
                var resultado = await _recibosCajaService.GetAllAsync(desde, hasta, metodoPago, search);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener recibos: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna el detalle de un recibo por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReciboEntity>> GetById(int id)
        {
            try
            {
                var recibo = await _recibosCajaService.GetByIdAsync(id);
                if (recibo == null)
                    return NotFound(new { mensaje = $"No se encontró el recibo con ID {id}" });

                return Ok(recibo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el recibo: {ex.Message}" });
            }
        }
    }
}
