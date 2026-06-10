using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;

        public ReportesController(IReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        /// <summary>
        /// GET /api/reportes/dashboard
        /// Retorna KPIs agregados: ventas (hoy/semana/mes), top 10 productos,
        /// resumen CxC, stock bajo y últimas facturas.
        /// </summary>
        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashboardDTO>> GetDashboard()
        {
            try
            {
                var resultado = await _reportesService.GetDashboardAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener dashboard: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/balance-minimo
        /// Estado de situación mínimo con activos, pasivos y patrimonio.
        /// </summary>
        [HttpGet("balance-minimo")]
        public async Task<ActionResult<BalanceMinimoReporteDTO>> GetBalanceMinimo()
        {
            try
            {
                var resultado = await _reportesService.GetBalanceMinimoAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener balance mínimo: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/ventas?desde=&amp;hasta=&amp;idCliente=&amp;idProducto=
        /// Reporte de ventas por rango de fechas con filtros opcionales.
        /// </summary>
        [HttpGet("Ventas")]
        public async Task<ActionResult<VentasReporteDTO>> GetVentas(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta,
            [FromQuery] string? idCliente,
            [FromQuery] int? idProducto)
        {
            try
            {
                Guid? clienteGuid = null;
                if (!string.IsNullOrEmpty(idCliente) && Guid.TryParse(idCliente, out var parsed))
                {
                    clienteGuid = parsed;
                }

                var resultado = await _reportesService.GetVentasAsync(desde, hasta, clienteGuid, idProducto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener reporte de ventas: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/cxc-aging?idCliente=
        /// Reporte de antigüedad de cuentas por cobrar en 4 buckets.
        /// </summary>
        [HttpGet("cxc-aging")]
        public async Task<ActionResult<CxcAgingReporteDTO>> GetCxcAging(
            [FromQuery] string? idCliente)
        {
            try
            {
                Guid? clienteGuid = null;
                if (!string.IsNullOrEmpty(idCliente) && Guid.TryParse(idCliente, out var parsed))
                {
                    clienteGuid = parsed;
                }

                var resultado = await _reportesService.GetCxcAgingAsync(clienteGuid);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener reporte CxC aging: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/inventario?idCategoria=&amp;stockThreshold=
        /// Reporte de inventario con niveles de stock y alertas.
        /// </summary>
        [HttpGet("Inventario")]
        public async Task<ActionResult<InventarioReporteDTO>> GetInventario(
            [FromQuery] int? idCategoria,
            [FromQuery] int stockThreshold = 10)
        {
            try
            {
                var resultado = await _reportesService.GetInventarioAsync(idCategoria, stockThreshold);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener reporte de inventario: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/movimientos?desde=&amp;hasta=&amp;idProducto=&amp;tipoMovimiento=
        /// Reporte de movimientos de producto con filtros.
        /// </summary>
        [HttpGet("Movimientos")]
        public async Task<ActionResult<MovimientosReporteDTO>> GetMovimientos(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta,
            [FromQuery] int? idProducto,
            [FromQuery] int? tipoMovimiento)
        {
            try
            {
                var resultado = await _reportesService.GetMovimientosAsync(desde, hasta, idProducto, tipoMovimiento);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener reporte de movimientos: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/movimientos-diarios
        /// Resumen diario: facturación, movimientos de MP y productos, CxC y alertas.
        /// </summary>
        [HttpGet("movimientos-diarios")]
        public async Task<ActionResult<MovimientosDiariosDTO>> GetMovimientosDiarios()
        {
            try
            {
                var resultado = await _reportesService.GetMovimientosDiariosAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener movimientos diarios: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /api/reportes/historial-cliente/{idCliente}
        /// Historial de compras de un cliente con totales y saldo CxC.
        /// </summary>
        [HttpGet("historial-cliente/{idCliente}")]
        public async Task<ActionResult<ClienteReporteDTO>> GetHistorialCliente(Guid idCliente)
        {
            try
            {
                var resultado = await _reportesService.GetClienteAsync(idCliente);
                if (resultado == null)
                    return NotFound(new { mensaje = $"No se encontró el cliente con ID {idCliente}" });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener historial del cliente: {ex.Message}" });
            }
        }
    }
}
