using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasMateriaPrimaController(CategoriasMateriaPrimaService categoriasMateriaPrimaService) : Controller
    {
        private readonly CategoriasMateriaPrimaService _categoriasMateriaPrimaService = categoriasMateriaPrimaService;

        [HttpPost]
        [Route("CrearCategoria")]
        public async Task<IActionResult> CrearCategoria(CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO)
        {
            await _categoriasMateriaPrimaService.CrearCategoriaAsync(categoriasMateriaPrimaDTO, GetUserId());
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerCategorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var categorias = await _categoriasMateriaPrimaService.ObtenerCategoriasAsync();
            return Ok(categorias);
        }

        [HttpPut]
        [Route("ActualizarEstadoCategorias")]
        public async Task<IActionResult> ActualizarEstadoCategorias([FromBody]MateriaPrimaActualizarEstadoDTO categoriasMateriaPrimaActualizarEstadoDTO)
        {
            try
            {
                await _categoriasMateriaPrimaService.ActualizarEstadoCategorias(categoriasMateriaPrimaActualizarEstadoDTO, GetUserId());
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarCategorias/{id}")]
        public async Task<IActionResult> ActualizarCategorias(int id, [FromBody]CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO)
        {
            try
            {
                await _categoriasMateriaPrimaService.ActualizarCategorias(id, categoriasMateriaPrimaDTO, GetUserId());
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
            }
        }

        [HttpGet]
        [Route("ObtenerCategoriaPorId/{idCategoria}")]
        public async Task<IActionResult> ObtenerCategoriaPorId(int idCategoria)
        {
            try
            {
                var categoria = await _categoriasMateriaPrimaService.ObtenerPorIdCategoria(idCategoria);
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("no se encontro", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
            }
        }

        [HttpGet]
        [Route("ObtenerCategoriasDisponibles")]
        public async Task<IActionResult> ObtenerCategoriasDisponibles()
        {
            var categoriasDisponibles = await _categoriasMateriaPrimaService.ObtenerCategoriasDisponibles();
            return Ok(categoriasDisponibles);
        }

        [HttpDelete]
        [Route("EliminarCategoria/{idCategoria}")]
        public async Task<IActionResult> EliminarCategoria(int idCategoria)
        {
            try
            {
                await _categoriasMateriaPrimaService.EliminarCategoriaAsync(idCategoria, GetUserId());
                return Ok(new { mensaje = "Categoría eliminada correctamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { mensaje = ex.Message });
                throw;
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
