using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController(ProveedoresService proveedoresService) : Controller
    {
        private readonly ProveedoresService _proveedoresService = proveedoresService;

        [HttpPost]
        [Route("CrearProveedor")]
        public async Task<IActionResult> CrearProveedor([FromBody] ProveedoresDTO proveedoresDTO)
        {
            try
            {
                await _proveedoresService.CrearProveedorAsync(proveedoresDTO, GetUserId());
                return Ok(new { success = true, message = "Proveedor creado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerProveedores")]
        public async Task<IActionResult> ObtenerProveedores()
        {
            try
            {
                var proveedores = await _proveedoresService.ObtenerProveedoresAsync();
                return Ok(new { success = true, message = "Proveedores obtenidos correctamente", data = proveedores });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerProveedorPorId/{id:guid}")]
        public async Task<IActionResult> ObtenerProveedorPorId(Guid id)
        {
            try
            {
                var proveedor = await _proveedoresService.ObtenerProveedorPorIdAsync(id);
                return Ok(new { success = true, message = "Proveedor obtenido correctamente", data = proveedor });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarEstadoProveedor")]
        public async Task<IActionResult> ActualizarEstadoProveedor([FromBody] ActualizarEstadoTipoGuidDTO actualizarEstadoDTO)
        {
            try
            {
                await _proveedoresService.ActualizarEstadoProveedor(actualizarEstadoDTO, GetUserId());
                return Ok(new { success = true, message = "Estado del proveedor actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarProveedor/{idProveedor:guid}")]
        public async Task<IActionResult> ActualizarProveedor(Guid idProveedor, [FromBody] ProveedoresDTO proveedoresDTO)
        {
            try
            {
                await _proveedoresService.ActualizarProveedor(idProveedor, proveedoresDTO, GetUserId());
                return Ok(new { success = true, message = "Proveedor actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerProveedoresDisponibles")]
        public async Task<IActionResult> ObtenerProveedoresDisponibles()
        {
            try
            {
                var proveedores = await _proveedoresService.ObtnenerProveedoresDisponibles();
                return Ok(new { success = true, message = "Proveedores disponibles obtenidos correctamente", data = proveedores });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarProveedor/{idProveedor}")]
        public async Task<IActionResult> EliminarProveedor(Guid idProveedor)
        {
            try
            {
                await _proveedoresService.EliminarProveedorAsync(idProveedor, GetUserId());
                return Ok(new { success = true, message = "Proveedor eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
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
