using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Caja;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        [HttpGet("Estado")]
        public async Task<ActionResult<CajaAperturaDTO>> ObtenerEstadoActual()
        {
            try
            {
                var caja = await _cajaService.ObtenerCajaActivaAsync();
                return caja == null ? NotFound(new { mensaje = "No hay una caja abierta" }) : Ok(caja);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener la caja: {ex.Message}" });
            }
        }

        [HttpPost("Abrir")]
        public async Task<ActionResult<CajaAperturaDTO>> AbrirCaja([FromBody] AbrirCajaDTO dto)
        {
            try
            {
                var resultado = await _cajaService.AbrirCajaAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al abrir la caja: {ex.Message}" });
            }
        }

        [HttpPost("Cerrar")]
        public async Task<ActionResult<CajaCierreDTO>> CerrarCaja([FromBody] CerrarCajaDTO dto)
        {
            try
            {
                var resultado = await _cajaService.CerrarCajaAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al cerrar la caja: {ex.Message}" });
            }
        }

        [HttpPost("Egreso")]
        public async Task<ActionResult<CajaMovimientoDTO>> RegistrarEgreso([FromBody] RegistrarEgresoDTO dto)
        {
            try
            {
                var resultado = await _cajaService.RegistrarEgresoAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al registrar el egreso: {ex.Message}" });
            }
        }

        [HttpGet("Movimientos")]
        public async Task<ActionResult<List<CajaMovimientoDTO>>> ObtenerMovimientos(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? tipoMovimiento)
        {
            try
            {
                var resultado = await _cajaService.ObtenerMovimientosAsync(desde, hasta, tipoMovimiento);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener movimientos: {ex.Message}" });
            }
        }

        [HttpGet("Reporte/Diario")]
        public async Task<ActionResult<CajaReporteDTO>> ReporteDiario()
        {
            try
            {
                return Ok(await _cajaService.ObtenerReporteDiarioAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el reporte diario: {ex.Message}" });
            }
        }

        [HttpGet("Reporte/Semanal")]
        public async Task<ActionResult<CajaReporteDTO>> ReporteSemanal()
        {
            try
            {
                return Ok(await _cajaService.ObtenerReporteSemanalAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el reporte semanal: {ex.Message}" });
            }
        }

        [HttpGet("Reporte/Mensual")]
        public async Task<ActionResult<CajaReporteDTO>> ReporteMensual()
        {
            try
            {
                return Ok(await _cajaService.ObtenerReporteMensualAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el reporte mensual: {ex.Message}" });
            }
        }
    }
}
