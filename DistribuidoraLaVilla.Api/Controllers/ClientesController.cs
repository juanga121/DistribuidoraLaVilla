using System.Security.Claims;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController(ClientesService clientesService) : Controller
    {
        private readonly ClientesService _clientesService = clientesService;

        [HttpPost]
        [Route("CrearCliente")]
        public async Task<IActionResult> CrearCliente([FromBody] ClientesDTO clientesDTO)
        {
            try
            {
                var idUsuario = GetUserId();
                await _clientesService.CrearClienteAsync(clientesDTO, idUsuario);
                return Ok(new { success = true, message = "Cliente creado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerClientes")]
        public async Task<IActionResult> ObtenerClientes()
        {
            try
            {
                var clientes = await _clientesService.ObtenerClientesAsync();
                return Ok(new { success = true, message = "Clientes obtenidos correctamente", data = clientes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerClientePorId/{id:guid}")]
        public async Task<IActionResult> ObtenerClientePorId(Guid id)
        {
            try
            {
                var cliente = await _clientesService.ObtenerClientePorIdAsync(id);
                return Ok(new { success = true, message = "Cliente obtenido correctamente", data = cliente });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarEstadoCliente")]
        public async Task<IActionResult> ActualizarEstadoCliente([FromBody] ActualizarEstadoTipoGuidDTO actualizarEstadoDTO)
        {
            try
            {
                var idUsuario = GetUserId();
                await _clientesService.ActualizarEstadoCliente(actualizarEstadoDTO, idUsuario);
                return Ok(new { success = true, message = "Estado del cliente actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarCliente/{idCliente:guid}")]
        public async Task<IActionResult> ActualizarCliente(Guid idCliente, [FromBody] ClientesDTO clientesDTO)
        {
            try
            {
                var idUsuario = GetUserId();
                await _clientesService.ActualizarCliente(idCliente, clientesDTO, idUsuario);
                return Ok(new { success = true, message = "Cliente actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerClientesDisponibles")]
        public async Task<IActionResult> ObtenerClientesDisponibles()
        {
            try
            {
                var clientes = await _clientesService.ObtenerClientesDisponibles();
                return Ok(new { success = true, message = "Clientes disponibles obtenidos correctamente", data = clientes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarCliente/{idCliente}")]
        public async Task<IActionResult> EliminarCliente(Guid idCliente)
        {
            try
            {
                var idUsuario = GetUserId();
                await _clientesService.EliminarClienteAsync(idCliente, idUsuario);
                return Ok(new { success = true, message = "Cliente eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Extrae el ID del usuario desde el JWT (ClaimTypes.NameIdentifier).
        /// Si no hay autenticación, retorna Guid.Empty como placeholder.
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
