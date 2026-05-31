using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Api.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosProductosController : ControllerBase
    {
        private readonly MovimientosProductosService _movimientosService;

        public MovimientosProductosController(MovimientosProductosService movimientosService)
        {
            _movimientosService = movimientosService;
        }

        /// <summary>
        /// Registra un nuevo movimiento de producto (Entrada, Venta, Ajuste, Devolución, Vencimiento)
        /// </summary>
        /// <param name="dto">Datos del movimiento a registrar</param>
        /// <returns>Información del movimiento registrado con stock anterior y nuevo</returns>
        [HttpPost("RegistrarMovimiento")]
        public async Task<ActionResult<RespuestaMovimientoProductoDTO>> RegistrarMovimiento([FromBody] CrearMovimientoProductoDTO dto)
        {
            try
            {
                var resultado = await _movimientosService.RegistrarMovimientoAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = $"Movimiento de tipo '{resultado.TipoMovimientoNombre}' registrado correctamente",
                    data = resultado
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error de validación",
                    errors = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene todos los movimientos de productos
        /// </summary>
        /// <returns>Lista completa de movimientos</returns>
        [HttpGet("ObtenerMovimientos")]
        public async Task<ActionResult<List<MovimientosProductosEntity>>> ObtenerMovimientos()
        {
            var movimientos = await _movimientosService.ObtenerMovimientosAsync();
            return Ok(new
            {
                success = true,
                message = "Movimientos obtenidos correctamente",
                data = movimientos
            });
        }

        /// <summary>
        /// Obtiene un movimiento específico por ID
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <returns>Información detallada del movimiento</returns>
        [HttpGet("ObtenerMovimientoPorId/{id}")]
        public async Task<ActionResult<MovimientosProductosEntity>> ObtenerMovimientoPorId(int id)
        {
            var movimiento = await _movimientosService.ObtenerMovimientoPorIdAsync(id);
            if (movimiento == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró el movimiento con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Movimiento obtenido correctamente",
                data = movimiento
            });
        }

        /// <summary>
        /// Obtiene todos los movimientos de un lote específico
        /// </summary>
        /// <param name="idLote">ID del lote de producto</param>
        /// <returns>Lista de movimientos del lote</returns>
        [HttpGet("ObtenerMovimientosPorLote/{idLote}")]
        public ActionResult<List<MovimientosProductosEntity>> ObtenerMovimientosPorLote(int idLote)
        {
            var movimientos = _movimientosService.ObtenerMovimientosPorLote(idLote);
            return Ok(new
            {
                success = true,
                message = $"Movimientos del lote {idLote} obtenidos correctamente",
                data = movimientos
            });
        }

        /// <summary>
        /// Obtiene movimientos filtrados por tipo (1=Entrada, 2=Venta, 3=Ajuste, 4=Devolución, 5=Vencimiento)
        /// </summary>
        /// <param name="tipoMovimiento">Tipo de movimiento (1-5)</param>
        /// <returns>Lista de movimientos del tipo especificado</returns>
        [HttpGet("ObtenerMovimientosPorTipo/{tipoMovimiento}")]
        public ActionResult<List<MovimientosProductosEntity>> ObtenerMovimientosPorTipo(int tipoMovimiento)
        {
            if (tipoMovimiento < 1 || tipoMovimiento > 5)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El tipo de movimiento debe estar entre 1 (Entrada) y 5 (Vencimiento)"
                });
            }

            var movimientos = _movimientosService.ObtenerMovimientosPorTipo(tipoMovimiento);
            return Ok(new
            {
                success = true,
                message = "Movimientos obtenidos correctamente",
                data = movimientos
            });
        }

        /// <summary>
        /// Obtiene movimientos en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial (formato: yyyy-MM-dd)</param>
        /// <param name="fechaFin">Fecha final (formato: yyyy-MM-dd)</param>
        /// <returns>Lista de movimientos en el rango especificado</returns>
        [HttpGet("ObtenerMovimientosPorFechas")]
        public ActionResult<List<MovimientosProductosEntity>> ObtenerMovimientosPorFechas(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaFin < fechaInicio)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "La fecha final no puede ser anterior a la fecha inicial"
                });
            }

            var movimientos = _movimientosService.ObtenerMovimientosPorFechas(fechaInicio, fechaFin);
            return Ok(new
            {
                success = true,
                message = $"Movimientos entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}",
                data = movimientos
            });
        }

        /// <summary>
        /// Obtiene movimientos de un cliente específico
        /// </summary>
        /// <param name="idCliente">ID del cliente</param>
        /// <returns>Lista de movimientos del cliente</returns>
        [HttpGet("ObtenerMovimientosPorCliente/{idCliente}")]
        public ActionResult<List<MovimientosProductosEntity>> ObtenerMovimientosPorCliente(Guid idCliente)
        {
            var movimientos = _movimientosService.ObtenerMovimientosPorCliente(idCliente);
            return Ok(new
            {
                success = true,
                message = "Movimientos del cliente obtenidos correctamente",
                data = movimientos
            });
        }

        /// <summary>
        /// Obtiene movimientos de un proveedor específico
        /// </summary>
        /// <param name="idProveedor">ID del proveedor</param>
        /// <returns>Lista de movimientos del proveedor</returns>
        [HttpGet("ObtenerMovimientosPorProveedor/{idProveedor}")]
        public ActionResult<List<MovimientosProductosEntity>> ObtenerMovimientosPorProveedor(Guid idProveedor)
        {
            var movimientos = _movimientosService.ObtenerMovimientosPorProveedor(idProveedor);
            return Ok(new
            {
                success = true,
                message = "Movimientos del proveedor obtenidos correctamente",
                data = movimientos
            });
        }

        /// <summary>
        /// Actualiza el estado de un movimiento (ej: 0=Anulado, 1=Activo)
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <param name="nuevoEstado">Nuevo estado (0 o 1)</param>
        /// <returns>Confirmación de la actualización</returns>
        [HttpPut("ActualizarEstadoMovimiento")]
        public async Task<ActionResult> ActualizarEstadoMovimiento([FromQuery] int id, [FromQuery] int nuevoEstado)
        {
            var resultado = await _movimientosService.ActualizarEstadoMovimientoAsync(id, nuevoEstado);
            if (!resultado)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró el movimiento con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Estado del movimiento actualizado a {nuevoEstado}"
            });
        }

        /// <summary>
        /// Elimina (desactiva) un movimiento
        /// </summary>
        /// <param name="id">ID del movimiento a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("EliminarMovimiento/{id}")]
        public async Task<ActionResult> EliminarMovimiento(int id)
        {
            var resultado = await _movimientosService.EliminarMovimientoAsync(id);
            if (!resultado)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No se encontró el movimiento con ID {id}"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Movimiento eliminado (desactivado) correctamente"
            });
        }

        /// <summary>
        /// Obtiene el catálogo de tipos de movimiento disponibles
        /// </summary>
        /// <returns>Lista de tipos de movimiento con ID, nombre y descripción</returns>
        [HttpGet("ObtenerTiposMovimiento")]
        public ActionResult<List<object>> ObtenerTiposMovimiento()
        {
            var tipos = _movimientosService.ObtenerTiposMovimiento();
            return Ok(new
            {
                success = true,
                message = "Tipos de movimiento obtenidos correctamente",
                data = tipos
            });
        }
    }
}
