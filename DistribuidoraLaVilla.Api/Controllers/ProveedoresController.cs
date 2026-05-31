using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

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
            await _proveedoresService.CrearProveedorAsync(proveedoresDTO);
            return Ok("Proveedor agregado con exito");
        }

        [HttpGet]
        [Route("ObtenerProveedores")]
        public async Task<IActionResult> ObtenerProveedores()
        {
            var proveedores = await _proveedoresService.ObtenerProveedoresAsync();
            return Ok(proveedores);
        }

        [HttpGet]
        [Route("ObtenerProveedorPorId/{id:guid}")]
        public async Task<IActionResult> ObtenerProveedorPorId(Guid id)
        {
            var proveedor = await _proveedoresService.ObtenerProveedorPorIdAsync(id);
            return Ok(proveedor);
        }

        [HttpPut]
        [Route("ActualizarEstadoProveedor")]
        public async Task<IActionResult> ActualizarEstadoProveedor([FromBody] ActualizarEstadoTipoGuidDTO actualizarEstadoDTO)
        {
            await _proveedoresService.ActualizarEstadoProveedor(actualizarEstadoDTO);
            return Ok("Estado del proveedor actualizado con exito");
        }

        [HttpPut]
        [Route("ActualizarProveedor/{idProveedor:guid}")]
        public async Task<IActionResult> ActualizarProveedor(Guid idProveedor, [FromBody] ProveedoresDTO proveedoresDTO)
        {
            await _proveedoresService.ActualizarProveedor(idProveedor, proveedoresDTO);
            return Ok("Proveedor actualizado con exito");
        }

        [HttpGet]
        [Route("ObtenerProveedoresDisponibles")]
        public async Task<IActionResult> ObtenerProveedoresDisponibles()
        {
            var proveedores = await _proveedoresService.ObtnenerProveedoresDisponibles();
            return Ok(proveedores);
        }

        [HttpDelete]
        [Route("EliminarProveedor/{idProveedor}")]
        public async Task<IActionResult> EliminarProveedor(Guid idProveedor)
        {
            await _proveedoresService.EliminarProveedorAsync(idProveedor);
            return Ok("Proveedor eliminado con exito");
        }
    }
}
