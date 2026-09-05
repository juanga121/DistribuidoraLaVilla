using DistribuidoraLaVilla.Application.Services.Compras;
using DistribuidoraLaVilla.Domain.DTOS.Compras;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenCompraController : ControllerBase
    {
        private readonly OrdenCompraService _service;

        public OrdenCompraController(OrdenCompraService service)
        {
            _service = service;
        }

        /// <summary>
        /// Crea una nueva orden de compra.
        /// </summary>
        [HttpPost("Crear")]
        public async Task<ActionResult<OrdenCompraResponseDTO>> Crear([FromBody] CrearOrdenCompraDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var resultado = await _service.CrearOrdenCompraAsync(dto, userId);
                return Ok(resultado);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno al crear la orden de compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna todas las órdenes de compra.
        /// </summary>
        [HttpGet("ObtenerTodas")]
        public async Task<ActionResult<List<OrdenCompraResponseDTO>>> ObtenerTodas()
        {
            try
            {
                var resultado = await _service.ObtenerOrdenesCompraAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener órdenes de compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Registra la recepción e invoice de una orden aprobada.
        /// </summary>
        [HttpPost("Recibir/{id}")]
        public async Task<ActionResult> Recibir(int id, [FromBody] RegistrarRecepcionCompraDTO dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.RegistrarRecepcionCompraAsync(id, dto, userId);
                return Ok(new { mensaje = "Recepción registrada correctamente" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno al registrar la recepción de compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna una orden de compra por su ID, incluyendo detalles.
        /// </summary>
        [HttpGet("ObtenerPorId/{id}")]
        public async Task<ActionResult<OrdenCompraResponseDTO>> ObtenerPorId(int id)
        {
            try
            {
                var resultado = await _service.ObtenerOrdenCompraPorIdAsync(id);
                if (resultado == null)
                    return NotFound(new { mensaje = $"No se encontró la orden de compra con ID {id}" });

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener la orden de compra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Retorna el comprobante de recepción de compra de una orden (CA06),
        /// para imprimir en térmica o convencional.
        /// </summary>
        [HttpGet("ObtenerComprobante/{idOrdenCompra}")]
        public async Task<ActionResult<ComprobanteCompraDTO>> ObtenerComprobante(int idOrdenCompra)
        {
            try
            {
                var resultado = await _service.ObtenerComprobanteRecepcionAsync(idOrdenCompra);
                if (resultado == null)
                    return NotFound(new { mensaje = $"No se encontró el comprobante para la orden con ID {idOrdenCompra}" });

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el comprobante: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza el estado de una orden de compra.
        /// </summary>
        [HttpPut("CambiarEstado")]
        public async Task<ActionResult> CambiarEstado([FromBody] ActualizarEstadoOrdenDTO dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.ActualizarEstadoOrdenAsync(dto.Id, dto.EstadoNuevo, userId);
                return Ok(new { mensaje = "Estado actualizado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno al cambiar estado: {ex.Message}" });
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
