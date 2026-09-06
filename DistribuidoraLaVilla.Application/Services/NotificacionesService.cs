using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.DTOS.CxC;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    /// <summary>
    /// Consolida las alertas operativas del sistema: cuentas por cobrar
    /// vencidas/próximas, cuentas por pagar vencidas/próximas, y stock de
    /// productos y materia prima próximos a vencer.
    /// </summary>
    public class NotificacionesService
    {
        private readonly ICuentasCobrarService _cxcService;
        private readonly ICuentasPagarService _cxpService;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository;

        public NotificacionesService(
            ICuentasCobrarService cxcService,
            ICuentasPagarService cxpService,
            IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
            IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository,
            IGenericRepository<ProductosEntity, int> productosRepository,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository)
        {
            _cxcService = cxcService;
            _cxpService = cxpService;
            _lotesProductosRepository = lotesProductosRepository;
            _lotesMateriaPrimaRepository = lotesMateriaPrimaRepository;
            _productosRepository = productosRepository;
            _materiaPrimaRepository = materiaPrimaRepository;
        }

        /// <summary>
        /// Obtiene el resumen de alertas. Incluye vencidas y las que vencen
        /// dentro de los próximos <paramref name="dias"/> días.
        /// </summary>
        public async Task<NotificacionesResumenDTO> ObtenerResumenAsync(int dias = 30)
        {
            var hoy = DateTime.Now.Date;
            var limite = hoy.AddDays(dias);

            // CxC: pendientes con saldo (incluye vencidas y próximas).
            var cxcPendientes = await _cxcService.ObtenerPendientesAsync();
            var cxcFiltradas = cxcPendientes
                .Where(c => c.FechaVencimiento <= limite)
                .OrderBy(c => c.FechaVencimiento)
                .Select(c => new NotificacionCxcDTO
                {
                    Id = c.Id,
                    IdFactura = c.IdFactura,
                    ClienteNombre = c.ClienteNombre,
                    FechaVencimiento = c.FechaVencimiento,
                    MontoTotal = c.MontoTotal,
                    SaldoPendiente = c.SaldoPendiente,
                    MontoPagado = c.MontoPagado,
                    Estado = c.Estado,
                    DiasVencidos = c.DiasVencidos
                })
                .ToList();

            // CxP: vencidas y próximas (estado Pendiente/ParcialmentePagada con saldo).
            var cxpTodas = await _cxpService.ObtenerTodosAsync();
            var cxpFiltradas = cxpTodas
                .Where(c => c.SaldoPendiente > 0 &&
                            c.FechaVencimiento.HasValue &&
                            c.FechaVencimiento.Value <= limite)
                .OrderBy(c => c.FechaVencimiento)
                .Select(c => new NotificacionCxpDTO
                {
                    IdCuentaPagar = c.IdCuentaPagar,
                    ProveedorNombre = c.ProveedorNombre,
                    FechaVencimiento = c.FechaVencimiento!.Value,
                    MontoTotal = c.MontoTotal,
                    SaldoPendiente = c.SaldoPendiente,
                    Estado = c.Estado,
                    EstadoDescripcion = c.EstadoDescripcion,
                    DiasVencidos = c.DiasVencidos
                })
                .ToList();

            // Stock de productos con lotes que vencen dentro del rango (incluye vencidos).
            var productos = await _productosRepository.GetAllAsync();
            var lotesProductos = _lotesProductosRepository.GetByFilter(l =>
                l.Estado == 1 &&
                l.CantidadDisponible > 0 &&
                l.FechaVencimiento <= limite);

            var stockProductos = new List<NotificacionStockDTO>();
            foreach (var lote in lotesProductos.OrderBy(l => l.FechaVencimiento))
            {
                var producto = productos.FirstOrDefault(p => p.Id == lote.IdProducto);
                stockProductos.Add(new NotificacionStockDTO
                {
                    IdLote = lote.Id,
                    Nombre = producto?.Nombre ?? $"Producto #{lote.IdProducto}",
                    CantidadDisponible = lote.CantidadDisponible,
                    UnidadMedidaId = lote.IdUnidadMedida,
                    FechaVencimiento = lote.FechaVencimiento,
                    DiasParaVencer = (int)(lote.FechaVencimiento - hoy).TotalDays
                });
            }

            // Stock de materia prima con lotes que vencen dentro del rango (incluye vencidos).
            var materiasPrimas = await _materiaPrimaRepository.GetAllAsync();
            var lotesMateriaPrima = _lotesMateriaPrimaRepository.GetByFilter(l =>
                l.Estado == 1 &&
                l.CantidadDisponible > 0 &&
                l.FechaVencimiento <= limite);

            var stockMateriaPrima = new List<NotificacionStockDTO>();
            foreach (var lote in lotesMateriaPrima.OrderBy(l => l.FechaVencimiento))
            {
                var mp = materiasPrimas.FirstOrDefault(p => p.Id == lote.IdMateria);
                stockMateriaPrima.Add(new NotificacionStockDTO
                {
                    IdLote = lote.Id,
                    Nombre = mp?.Nombre ?? $"Materia prima #{lote.IdMateria}",
                    CantidadDisponible = lote.CantidadDisponible,
                    UnidadMedidaId = lote.IdUnidadMedida,
                    FechaVencimiento = lote.FechaVencimiento,
                    DiasParaVencer = (int)(lote.FechaVencimiento - hoy).TotalDays
                });
            }

            return new NotificacionesResumenDTO
            {
                FechaConsulta = DateTime.Now,
                DiasAnticipacion = dias,
                TotalAlertas = cxcFiltradas.Count + cxpFiltradas.Count + stockProductos.Count + stockMateriaPrima.Count,
                CuentasCobrar = cxcFiltradas,
                CuentasPagar = cxpFiltradas,
                StockProductos = stockProductos,
                StockMateriaPrima = stockMateriaPrima
            };
        }
    }
}