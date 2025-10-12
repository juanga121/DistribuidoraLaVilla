using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

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
            await _categoriasMateriaPrimaService.CrearCategoriaAsync(categoriasMateriaPrimaDTO);
            return Ok("Categoria agregada con exito");
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
                await _categoriasMateriaPrimaService.ActualizarEstadoCategorias(categoriasMateriaPrimaActualizarEstadoDTO);
                return Ok("Estado de la categoria actualizado con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("ActualizarCategorias/{id}")]
        public async Task<IActionResult> ActualizarCategorias(int id, [FromBody]CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO)
        {
            try
            {
                await _categoriasMateriaPrimaService.ActualizarCategorias(id, categoriasMateriaPrimaDTO);
                return Ok("Categoria actualizada con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("ObtenerCategoriaPorId/{idCategoria}")]
        public async Task<IActionResult> ObtenerCategoriaPorId(int idCategoria)
        {
            var categoria = await _categoriasMateriaPrimaService.ObtenerPorIdCategoria(idCategoria);
            return Ok(categoria);
        }
    }
}
