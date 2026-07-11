using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Facturacion;
using DistribuidoraLaVilla.Domain.DTOS.CxC;
using DistribuidoraLaVilla.Domain.DTOS.Facturacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CxcController : ControllerBase
    {
        private readonly ICuentasCobrarService _cxcService;
        private readonly FacturaService _facturaService;

        public CxcController(
            ICuentasCobrarService cxcService,
            FacturaService facturaService)
        {
            _cxcService = cxcService;
            _facturaService = facturaService;
        }

        /// <summary>
        /// Retorna todas las cuentas por cobrar pendientes (SaldoPendiente > 0).
        /// </summary>
        [HttpGet("Pendientes")]
        public async Task<ActionResult<List<CuentaCobrarDTO>>> ObtenerPendientes()
        {
            try
            {
                var resultado = await _cxcService.ObtenerPendientesAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener cuentas pendientes: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna las cuentas por cobrar vencidas (FechaVencimiento menor a hoy y SaldoPendiente > 0).
        /// </summary>
        [HttpGet("Vencidas")]
        public async Task<ActionResult<List<CuentaCobrarDTO>>> ObtenerVencidas()
        {
            try
            {
                var resultado = await _cxcService.ObtenerVencidasAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener cuentas vencidas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna todas las cuentas por cobrar pagadas (Estado = 2 o SaldoPendiente = 0).
        /// </summary>
        [HttpGet("Pagadas")]
        public async Task<ActionResult<List<CuentaCobrarDTO>>> ObtenerPagadas()
        {
            try
            {
                var resultado = await _cxcService.ObtenerPagadasAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener cuentas pagadas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna el estado de cuenta detallado de un cliente.
        /// </summary>
        [HttpGet("EstadoCuenta/{idCliente}")]
        public async Task<ActionResult<EstadoCuentaDTO>> ObtenerEstadoCuenta(string idCliente)
        {
            try
            {
                var resultado = await _cxcService.ObtenerEstadoCuentaAsync(idCliente);
                if (resultado == null)
                    return NotFound(new { mensaje = $"No se encontró el cliente con ID {idCliente}" });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener estado de cuenta: {ex.Message}" });
            }
        }

        /// <summary>
        /// Registra un pago (total o parcial) contra una cuenta por cobrar.
        /// </summary>
        [HttpPost("RegistrarPago")]
        public async Task<ActionResult<PagoResponseDTO>> RegistrarPago([FromBody] RegistrarPagoDTO dto)
        {
            try
            {
                var resultado = await _cxcService.RegistrarPagoAsync(dto);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error interno al registrar el pago: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Crea una factura a crédito con validación de límite y cuenta por cobrar.
        /// </summary>
        [HttpPost("CrearFacturaCredito")]
        public async Task<ActionResult<FacturaResponseDTO>> CrearFacturaCredito([FromBody] CrearFacturaCreditoDTO dto)
        {
            try
            {
                var resultado = await _facturaService.CrearFacturaCreditoAsync(dto);

                if (!resultado.Exitoso)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error interno al crear la factura a crédito: {ex.Message}"
                });
            }
        }
    }
}
