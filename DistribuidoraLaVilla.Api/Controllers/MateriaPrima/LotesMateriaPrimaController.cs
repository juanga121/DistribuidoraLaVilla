using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesMateriaPrimaController(LotesMateriaPrimaService lotesMateriaPrimaService) : Controller
    {
        private readonly LotesMateriaPrimaService _lotesMateriaPrimaService = lotesMateriaPrimaService;

        [HttpPost]
        [Route("CrearLoteMateriaPrima")]
        public async Task<IActionResult> CrearLoteMateriaPrima([FromBody] LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            await _lotesMateriaPrimaService.CrearLoteMateriaPrimaAsync(lotesMateriaPrimaDTO);
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerLotesMateriaPrima")]
        public async Task<IActionResult> ObtenerLotesMateriaPrima()
        {
            var lotes = await _lotesMateriaPrimaService.ObtenerLotesMateriaPrimaAsync();
            return Ok(lotes);
        }

        [HttpGet]
        [Route("ObtenerLoteMateriaPrimaPorId/{id}")]
        public async Task<IActionResult> ObtenerLoteMateriaPrimaPorId(int id)
        {
            var lote = await _lotesMateriaPrimaService.ObtenerLoteMateriaPrimaPorIdAsync(id);
            return Ok(lote);
        }

        [HttpPut]
        [Route("ActualizarEstadoLoteMateriaPrima")]
        public async Task<IActionResult> ActualizarEstadoLoteMateriaPrima([FromBody] ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            await _lotesMateriaPrimaService.ActualizarEstadoLoteMateriaPrima(actualizarEstadoDTO);
            return Ok();
        }

        [HttpPut]
        [Route("ActualizarLoteMateriaPrima/{idLoteMateriaPrima}")]
        public async Task<IActionResult> ActualizarLoteMateriaPrima(int idLoteMateriaPrima, [FromBody] LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            await _lotesMateriaPrimaService.ActualizarLoteMateriaPrima(idLoteMateriaPrima, lotesMateriaPrimaDTO);
            return Ok();
        }

        [HttpGet]
        [Route("ObtenerLotesMateriaPrimaDisponibles")]
        public async Task<IActionResult> ObtenerLotesMateriaPrimaDisponibles()
        {
            var lotes = await _lotesMateriaPrimaService.ObtenerLotesMateriaPrimaDisponiblesAsync();
            return Ok(lotes);
        }

        [HttpDelete]
        [Route("EliminarLoteMateriaPrima/{idLote}")]
        public async Task<IActionResult> EliminarLote(int idLote)
        {
            await _lotesMateriaPrimaService.EliminarLoteMateriaPrimaAsync(idLote);
            return Ok();
        }
    }
}
