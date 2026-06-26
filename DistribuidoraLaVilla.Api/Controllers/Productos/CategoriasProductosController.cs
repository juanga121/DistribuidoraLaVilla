using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasProductosController(CategoriasProductosService categoriasProductosService) : Controller
    {
        private readonly CategoriasProductosService _categoriasProductosService = categoriasProductosService;

        [HttpPost]
        [Route("CrearCategoria")]
        public async Task<IActionResult> CrearCategoria(CategoriasProductosDTO categoriasProductosDTO)
        {
            await _categoriasProductosService.CrearCategoriaAsync(categoriasProductosDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerCategorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var categorias = await _categoriasProductosService.ObtenerCategoriasAsync();
            return Ok(categorias);
        }

        [HttpPut]
        [Route("ActualizarEstadoCategorias")]
        public async Task<IActionResult> ActualizarEstadoCategorias([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            try
            {
                await _categoriasProductosService.ActualizarEstadoCategorias(actualizarEstadoDTO, GetUserId());
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("ActualizarCategorias/{id}")]
        public async Task<IActionResult> ActualizarCategorias(int id, [FromBody] CategoriasProductosDTO categoriasProductosDTO)
        {
            await _categoriasProductosService.ActualizarCategorias(id, categoriasProductosDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerCategoriaPorId/{idCategoria}")]
        public async Task<IActionResult> ObtenerCategoriaPorId(int idCategoria)
        {
            var categoria = await _categoriasProductosService.ObtenerPorIdCategoria(idCategoria);
            return Ok(categoria);
        }

        [HttpGet]
        [Route("ObtenerCategoriasDisponibles")]
        public async Task<IActionResult> ObtenerCategoriasDisponibles()
        {
            var categoriasDisponibles = await _categoriasProductosService.ObtenerCategoriasDisponibles();
            return Ok(categoriasDisponibles);
        }

        [HttpDelete]
        [Route("EliminarCategoria/{idCategoria}")]
        public async Task<IActionResult> EliminarCategoria(int idCategoria)
        {
            await _categoriasProductosService.EliminarCategoriaAsync(idCategoria, GetUserId());
            return Ok();
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
