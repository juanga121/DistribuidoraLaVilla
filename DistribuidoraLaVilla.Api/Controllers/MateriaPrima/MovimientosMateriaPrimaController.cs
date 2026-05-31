using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers.MateriaPrima
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MovimientosMateriaPrimaController(MovimientosMateriaPrimaService movimientosService) : Controller
    {
        private readonly MovimientosMateriaPrimaService _movimientosService = movimientosService;

        /// <summary>
        /// Crea un nuevo movimiento de materia prima y actualiza el stock del lote
        /// </summary>
        /// <param name="movimientoDTO">Datos del movimiento a crear</param>
        /// <returns>Información del movimiento creado con stock anterior y nuevo</returns>
        /// <response code="200">Movimiento creado exitosamente</response>
        /// <response code="400">Error de validación o stock insuficiente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [Route("CrearMovimiento")]
        [ProducesResponseType(typeof(MovimientoMateriaPrimaResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearMovimiento([FromBody] MovimientoMateriaPrimaDTO movimientoDTO)
        {
            try
            {
                var resultado = await _movimientosService.CrearMovimientoAsync(movimientoDTO);
                return Ok(new 
                { 
                    mensaje = "Movimiento creado exitosamente",
                    movimiento = resultado
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errores = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear el movimiento", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los movimientos de materia prima
        /// </summary>
        /// <returns>Lista de todos los movimientos registrados</returns>
        /// <response code="200">Lista de movimientos obtenida exitosamente</response>
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

        /// <summary>
        /// Obtiene un movimiento específico por ID
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <returns>Datos del movimiento solicitado</returns>
        /// <response code="200">Movimiento encontrado</response>
        /// <response code="404">Movimiento no encontrado</response>
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

        /// <summary>
        /// Obtiene todos los movimientos de un lote específico
        /// </summary>
        /// <param name="idLote">ID del lote de materia prima</param>
        /// <returns>Lista de movimientos del lote</returns>
        /// <response code="200">Movimientos obtenidos exitosamente</response>
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

        /// <summary>
        /// Obtiene movimientos filtrados por tipo
        /// </summary>
        /// <param name="idTipoMovimiento">ID del tipo de movimiento (1=Entrada, 2=Consumo, 3=Ajuste, 4=Devolución, 5=Vencimiento)</param>
        /// <returns>Lista de movimientos del tipo especificado</returns>
        /// <response code="200">Movimientos obtenidos exitosamente</response>
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

        /// <summary>
        /// Obtiene movimientos en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <returns>Lista de movimientos en el rango especificado</returns>
        /// <response code="200">Movimientos obtenidos exitosamente</response>
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

        /// <summary>
        /// Obtiene la lista de tipos de movimiento disponibles
        /// </summary>
        /// <returns>Lista de tipos de movimiento con su ID y descripción</returns>
        /// <response code="200">Lista de tipos obtenida exitosamente</response>
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
