using DistribuidoraLaVilla.Application.Services.Facturacion;
using DistribuidoraLaVilla.Domain.DTOS.Facturacion;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturacionController(FacturaService facturaService) : ControllerBase
    {
        private readonly FacturaService _facturaService = facturaService;

        [HttpPost]
        [Route("CrearFacturaContado")]
        public async Task<IActionResult> CrearFacturaContado([FromBody] CrearFacturaDTO solicitud)
        {
            try
            {
                var resultado = await _facturaService.CrearFacturaContadoAsync(solicitud);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error interno al crear la factura: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Route("ObtenerFacturas")]
        public async Task<IActionResult> ObtenerFacturas()
        {
            try
            {
                var facturas = await _facturaService.ObtenerFacturasAsync();
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener facturas: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("ObtenerFacturaPorId/{id:int}")]
        public async Task<IActionResult> ObtenerFacturaPorId(int id)
        {
            try
            {
                var factura = await _facturaService.ObtenerFacturaPorIdAsync(id);
                if (factura == null)
                    return NotFound(new { mensaje = $"No se encontró la factura con ID {id}" });

                return Ok(factura);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener la factura: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("ObtenerFacturaTicket/{id:int}")]
        public async Task<IActionResult> ObtenerFacturaTicket(int id)
        {
            try
            {
                var ticket = await _facturaService.GenerarTicketAsync(id);
                if (ticket == null)
                    return NotFound(new { mensaje = $"No se encontró la factura con ID {id}" });

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al generar el ticket: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /api/Facturacion/ticketera
        /// Facturación rápida POS — cliente opcional (consumidor final por defecto)
        /// </summary>
        [HttpPost("ticketera")]
        public async Task<ActionResult<TicketeraResultDTO>> CrearTicketera([FromBody] CrearTicketeraDTO dto)
        {
            try
            {
                var idUsuario = GetUserId();
                var resultado = await _facturaService.CrearTicketeraAsync(dto, idUsuario);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al procesar venta POS: {ex.Message}" });
            }
        }

        /// <summary>
        /// Extrae el ID del usuario desde el JWT (ClaimTypes.NameIdentifier).
        /// Si no hay autenticación, retorna Guid.Empty como placeholder.
        /// TODO: reemplazar con autenticación real cuando esté implementada.
        /// </summary>
        private Guid GetUserId()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(nameIdentifier) && Guid.TryParse(nameIdentifier, out var userId))
                return userId;

            return Guid.Empty;
        }
    }
}
