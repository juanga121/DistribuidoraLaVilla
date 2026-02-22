using DistribuidoraLaVilla.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnidadMedidaController : Controller
    {
        private readonly UnidadMedidaService _unidadMedidaService;

        public UnidadMedidaController(UnidadMedidaService unidadMedidaService)
        {
            _unidadMedidaService = unidadMedidaService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerUnidadesMedida()
        {
            var unidades = await _unidadMedidaService.ObtenerUnidadesMedidaAsync();
            return Ok(unidades);
        }
    }
}
