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
            var idUsuario = GetUserId();
            await _clientesService.CrearClienteAsync(clientesDTO, idUsuario);
            return Ok("Cliente agregado con exito");
        }

        [HttpGet]
        [Route("ObtenerClientes")]
        public async Task<IActionResult> ObtenerClientes()
        {
            var clientes = await _clientesService.ObtenerClientesAsync();
            return Ok(clientes);
        }

        [HttpGet]
        [Route("ObtenerClientePorId/{id:guid}")]
        public async Task<IActionResult> ObtenerClientePorId(Guid id)
        {
            var cliente = await _clientesService.ObtenerClientePorIdAsync(id);
            return Ok(cliente);
        }

        [HttpPut]
        [Route("ActualizarEstadoCliente")]
        public async Task<IActionResult> ActualizarEstadoCliente([FromBody] ActualizarEstadoTipoGuidDTO actualizarEstadoDTO)
        {
            var idUsuario = GetUserId();
            await _clientesService.ActualizarEstadoCliente(actualizarEstadoDTO, idUsuario);
            return Ok("Estado del cliente actualizado con exito");
        }

        [HttpPut]
        [Route("ActualizarCliente/{idCliente:guid}")]
        public async Task<IActionResult> ActualizarCliente(Guid idCliente, [FromBody] ClientesDTO clientesDTO)
        {
            var idUsuario = GetUserId();
            await _clientesService.ActualizarCliente(idCliente, clientesDTO, idUsuario);
            return Ok("Cliente actualizado con exito");
        }

        [HttpGet]
        [Route("ObtenerClientesDisponibles")]
        public async Task<IActionResult> ObtenerClientesDisponibles()
        {
            var clientes = await _clientesService.ObtenerClientesDisponibles();
            return Ok(clientes);
        }

        [HttpDelete]
        [Route("EliminarCliente/{idCliente}")]
        public async Task<IActionResult> EliminarCliente(Guid idCliente)
        {
            var idUsuario = GetUserId();
            await _clientesService.EliminarClienteAsync(idCliente, idUsuario);
            return Ok("Cliente eliminado con exito");
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
