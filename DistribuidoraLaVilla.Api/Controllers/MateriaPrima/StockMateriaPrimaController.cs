using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockMateriaPrimaController : ControllerBase
    {
        private readonly StockMateriaPrimaService _stockService;

        public StockMateriaPrimaController(StockMateriaPrimaService stockService)
        {
            _stockService = stockService;
        }

        /// <summary>
        /// Obtiene el stock consolidado de todas las materias primas
        /// </summary>
        /// <returns>Lista de stock agrupado por materia prima con lotes disponibles ordenados por fecha de vencimiento</returns>
        [HttpGet]
        public async Task<ActionResult<List<StockMateriaPrimaDTO>>> ObtenerStockConsolidado()
        {
            var stock = await _stockService.ObtenerStockConsolidadoAsync();
            return Ok(new
            {
                success = true,
                message = "Stock consolidado obtenido correctamente",
                data = stock
            });
        }

        /// <summary>
        /// Obtiene el stock de una materia prima específica por ID
        /// </summary>
        /// <param name="id">ID de la materia prima</param>
        /// <returns>Stock de la materia prima con detalle de lotes</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<StockMateriaPrimaDTO>> ObtenerStockPorId(int id)
        {
            var stock = await _stockService.ObtenerStockPorIdAsync(id);
            if (stock == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró materia prima con ID {id} o está inactiva"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Stock obtenido correctamente",
                data = stock
            });
        }

        /// <summary>
        /// Obtiene materias primas con stock bajo (menor al mínimo especificado)
        /// </summary>
        /// <param name="minimo">Cantidad mínima de stock (default: 10)</param>
        /// <returns>Lista de materias primas con stock bajo</returns>
        [HttpGet("bajo")]
        public async Task<ActionResult<List<StockMateriaPrimaDTO>>> ObtenerStockBajo([FromQuery] decimal minimo = 10)
        {
            var stock = await _stockService.ObtenerStockBajoAsync(minimo);
            return Ok(new
            {
                success = true,
                message = $"Materias primas con stock inferior a {minimo}",
                data = stock
            });
        }

        /// <summary>
        /// Obtiene materias primas sin stock disponible
        /// </summary>
        /// <returns>Lista de materias primas con stock en cero</returns>
        [HttpGet("sin-stock")]
        public async Task<ActionResult<List<StockMateriaPrimaDTO>>> ObtenerSinStock()
        {
            var stock = await _stockService.ObtenerSinStockAsync();
            return Ok(new
            {
                success = true,
                message = "Materias primas sin stock",
                data = stock
            });
        }

        /// <summary>
        /// Obtiene lotes próximos a vencer
        /// </summary>
        /// <param name="dias">Días de anticipación para considerar próximo a vencer (default: 30)</param>
        /// <returns>Lista de materias primas con lotes próximos a vencer</returns>
        [HttpGet("proximos-vencer")]
        public async Task<ActionResult<List<StockMateriaPrimaDTO>>> ObtenerProximosAVencer([FromQuery] int dias = 30)
        {
            var stock = await _stockService.ObtenerProximosAVencerAsync(dias);
            return Ok(new
            {
                success = true,
                message = $"Materias primas con lotes que vencen en los próximos {dias} días",
                data = stock
            });
        }
    }
}
