using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MovimientosMateriaPrimaController(MovimientosMateriaPrimaService movimientosService) : Controller
    {
        private readonly MovimientosMateriaPrimaService _movimientosService = movimientosService;

        [HttpGet]
        [Route("ObtenerMovimientos")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ObtenerMovimientos()
        {
            try
            {
                var movimientos = await _movimientosService.ObtenerMovimientosAsync();
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMovimientosDetalle")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ObtenerMovimientosDetalle()
        {
            try
            {
                var movimientos = await _movimientosService.ObtenerMovimientosDetalleAsync();
                return Ok(new
                {
                    success = true,
                    message = "Movimientos obtenidos correctamente",
                    data = movimientos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMovimientoPorId/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerMovimientoPorId(int id)
        {
            try
            {
                var movimiento = await _movimientosService.ObtenerMovimientoPorIdAsync(id);
                return Ok(movimiento);
            }
            catch (Exception ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMovimientosPorLote/{idLote}")]
        [ProducesResponseType(200)]
        public IActionResult ObtenerMovimientosPorLote(int idLote)
        {
            try
            {
                var movimientos = _movimientosService.ObtenerMovimientosPorLoteAsync(idLote);
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMovimientosPorTipo/{idTipoMovimiento}")]
        [ProducesResponseType(200)]
        public IActionResult ObtenerMovimientosPorTipo(int idTipoMovimiento)
        {
            try
            {
                var movimientos = _movimientosService.ObtenerMovimientosPorTipoAsync(idTipoMovimiento);
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerMovimientosPorFechas")]
        [ProducesResponseType(200)]
        public IActionResult ObtenerMovimientosPorFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                var movimientos = _movimientosService.ObtenerMovimientosPorFechasAsync(fechaInicio, fechaFin);
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", detalle = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerTiposMovimiento")]
        [ProducesResponseType(200)]
        public IActionResult ObtenerTiposMovimiento()
        {
            var tipos = new[]
            {
                new { Id = 1, Nombre = "Entrada", Descripcion = "Entrada de materia prima al inventario" },
                new { Id = 2, Nombre = "Consumo", Descripcion = "Consumo de materia prima en producción" },
                new { Id = 3, Nombre = "Ajuste", Descripcion = "Ajuste de inventario (establece cantidad exacta)" },
                new { Id = 4, Nombre = "Devolución", Descripcion = "Devolución de materia prima al inventario" },
                new { Id = 5, Nombre = "Vencimiento", Descripcion = "Baja por vencimiento de materia prima" }
            };

            return Ok(tipos);
        }
    }
}
