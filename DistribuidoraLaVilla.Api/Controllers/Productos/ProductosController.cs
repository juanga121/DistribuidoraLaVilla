using System.Security.Claims;
using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController(ProductosService productosService) : Controller
    {
        private readonly ProductosService _productosService = productosService;

        [HttpPost]
        [Route("CrearProducto")]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoDTO productoDTO)
        {
            var idUsuario = GetUserId();
            await _productosService.CrearProductoAsync(productoDTO, idUsuario);
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerProductos")]
        public async Task<IActionResult> ObtenerProductos()
        {
            var result = await _productosService.ObtenerProductosAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("ObtenerProductoPorId/{idProducto}")]
        public async Task<IActionResult> ObtenerProductoPorId(int idProducto)
        {
            var result = await _productosService.ObtenerProductoPorId(idProducto);
            return Ok(result);
        }

        [HttpPut]
        [Route("ActualizarEstadoProducto")]
        public async Task<IActionResult> ActualizarEstadoProducto([FromBody] ProductoActualizarEstadoDTO productoActualizarEstadoDTO)
        {
            var idUsuario = GetUserId();
            await _productosService.ActualizarEstadoProducto(productoActualizarEstadoDTO, idUsuario);
            return Ok();
        }

        [HttpPut]
        [Route("ActualizarProducto/{idProducto}")]
        public async Task<IActionResult> ActualizarProducto(int idProducto, [FromBody] ProductoDTO productoDTO)
        {
            var idUsuario = GetUserId();
            await _productosService.ActualizarProducto(idProducto, productoDTO, idUsuario);
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerProductosDisponibles")]
        public async Task<IActionResult> ObtenerProductosDisponibles()
        {
            var productos = await _productosService.ObtenerProductosDisponibles();
            return Ok(productos);
        }

        [HttpDelete]
        [Route("EliminarProducto/{idProducto}")]
        public async Task<IActionResult> EliminarProducto(int idProducto)
        {
            var idUsuario = GetUserId();
            await _productosService.EliminarProductoAsync(idProducto, idUsuario);
            return Ok();
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
