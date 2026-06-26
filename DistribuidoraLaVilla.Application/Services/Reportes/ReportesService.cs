using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Reportes;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraLaVilla.Application.Services.Reportes
{
    public class ReportesService : IReportesService
    {
        private readonly IGenericRepository<FacturaEntity, int> _facturaRepo;
        private readonly IGenericRepository<DetalleFacturaEntity, int> _detalleRepo;
        private readonly IGenericRepository<ProductosEntity, int> _productoRepo;
        private readonly IGenericRepository<CuentasCobrarEntity, int> _cxcRepo;
        private readonly IGenericRepository<LotesProductosEntity, int> _loteRepo;
        private readonly IGenericRepository<ClientesEntity, Guid> _clienteRepo;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepo;
        private readonly IGenericRepository<CategoriasProductosEntity, int> _categoriaRepo;
        private readonly IGenericRepository<MovimientoEntity, int> _movimientoRepo;
        private readonly IGenericRepository<TipoFacturaEntity, int> _tipoFacturaRepo;
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movMPRepo;
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movProdRepo;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _loteMPRepo;
        private readonly IGenericRepository<PasivosEntity, int> _pasivosRepo;

        public ReportesService(
            IGenericRepository<FacturaEntity, int> facturaRepo,
            IGenericRepository<DetalleFacturaEntity, int> detalleRepo,
            IGenericRepository<ProductosEntity, int> productoRepo,
            IGenericRepository<CuentasCobrarEntity, int> cxcRepo,
            IGenericRepository<LotesProductosEntity, int> loteRepo,
            IGenericRepository<ClientesEntity, Guid> clienteRepo,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepo,
            IGenericRepository<CategoriasProductosEntity, int> categoriaRepo,
            IGenericRepository<MovimientoEntity, int> movimientoRepo,
            IGenericRepository<TipoFacturaEntity, int> tipoFacturaRepo,
            IGenericRepository<MovimientosMateriaPrimaEntity, int> movMPRepo,
            IGenericRepository<MovimientosProductosEntity, int> movProdRepo,
            IGenericRepository<LotesMateriaPrimaEntity, int> loteMPRepo,
            IGenericRepository<PasivosEntity, int> pasivosRepo)
        {
            _facturaRepo = facturaRepo;
            _detalleRepo = detalleRepo;
            _productoRepo = productoRepo;
            _cxcRepo = cxcRepo;
            _loteRepo = loteRepo;
            _clienteRepo = clienteRepo;
            _materiaPrimaRepo = materiaPrimaRepo;
            _categoriaRepo = categoriaRepo;
            _movimientoRepo = movimientoRepo;
            _tipoFacturaRepo = tipoFacturaRepo;
            _movMPRepo = movMPRepo;
            _movProdRepo = movProdRepo;
            _loteMPRepo = loteMPRepo;
            _pasivosRepo = pasivosRepo;
        }

        // ──────────────────────────────────────────────
        // T-03: Dashboard
        // ──────────────────────────────────────────────

        public async Task<DashboardDTO> GetDashboardAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;

            // Monday of current week (start of week)
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            // Start of current month
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var facturasQuery = _facturaRepo.GetQueryable().Where(f => f.Estado == 1);

            // ── Ventas Hoy ──
            var ventasHoy = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date == today)
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

            // ── Ventas Semana ──
            var ventasSemana = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date >= startOfWeek && f.Fecha.Value.Date < startOfWeek.AddDays(7))
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

            // ── Ventas Mes ──
            var ventasMes = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Year == today.Year && f.Fecha.Value.Month == today.Month)
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

            // ── Top 10 Productos ──
            var detallesQuery = _detalleRepo.GetQueryable();
            var productosQuery = _productoRepo.GetQueryable();

            var topProductos = await detallesQuery
                .Join(productosQuery,
                    d => d.IdProducto,
                    p => (int?)p.Id,
                    (d, p) => new { d, p })
                .GroupBy(x => new { x.p.Id, x.p.Nombre })
                .Select(g => new TopProductoDTO
                {
                    IdProducto = g.Key.Id,
                    NombreProducto = g.Key.Nombre ?? string.Empty,
                    TotalVendido = g.Sum(x => x.d.Subtotal ?? 0),
                    CantidadVendida = (int)(g.Sum(x => x.d.Cantidad ?? 0))
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(10)
                .ToListAsync();

            // ── CxC Resumen ──
            var cxcQuery = _cxcRepo.GetQueryable()
                .Where(c => c.SaldoPendiente > 0 && c.Estado == 1);

            var cxcPendiente = await cxcQuery
                .GroupBy(_ => 1)
                .Select(g => new CxcResumenDTO
                {
                    Total = g.Sum(c => c.SaldoPendiente ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new CxcResumenDTO();

            var cxcVencido = await cxcQuery
                .Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < now)
                .GroupBy(_ => 1)
                .Select(g => new CxcResumenDTO
                {
                    Total = g.Sum(c => c.SaldoPendiente ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new CxcResumenDTO();

            // ── Stock Bajo (threshold = 10) ──
            var stockBajo = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1)
                .GroupBy(l => new { l.IdProducto })
                .Select(g => new
                {
                    IdProducto = g.Key.IdProducto,
                    StockActual = g.Sum(l => l.CantidadDisponible)
                })
                .Where(g => g.StockActual < 10)
                .Join(productosQuery,
                    s => s.IdProducto,
                    p => p.Id,
                    (s, p) => new StockBajoDTO
                    {
                        IdProducto = s.IdProducto,
                        NombreProducto = p.Nombre ?? string.Empty,
                        StockActual = s.StockActual,
                        StockMinimo = 10
                    })
                .OrderBy(s => s.NombreProducto)
                .ToListAsync();

            // ── Últimas 10 Facturas ──
            var ultimasFacturas = await facturasQuery
                .Where(f => f.Fecha.HasValue)
                .OrderByDescending(f => f.Fecha)
                .Take(10)
                .Join(_clienteRepo.GetQueryable(),
                    f => f.IdCliente,
                    c => (Guid?)c.IdCliente,
                    (f, c) => new FacturaResumenDTO
                    {
                        IdFactura = f.Id,
                        Fecha = f.Fecha ?? DateTime.MinValue,
                        Cliente = c.Nombre ?? string.Empty,
                        Total = f.Total ?? 0
                    })
                .ToListAsync();

            return new DashboardDTO
            {
                VentasHoy = ventasHoy,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                TopProductos = topProductos,
                CxcPendiente = cxcPendiente,
                CxcVencido = cxcVencido,
                StockBajo = stockBajo,
                UltimasFacturas = ultimasFacturas
            };
        }

        // ──────────────────────────────────────────────
        // T-09: Balance mínimo
        // ──────────────────────────────────────────────

        public async Task<BalanceMinimoReporteDTO> GetBalanceMinimoAsync()
        {
            var cxcItems = await _cxcRepo.GetQueryable()
                .Where(c => c.Estado == 1 && (c.SaldoPendiente ?? 0) > 0)
                .Join(_clienteRepo.GetQueryable(),
                    c => c.IdCliente,
                    cl => (Guid?)cl.IdCliente,
                    (c, cl) => new BalanceCxcItemDTO
                    {
                        IdFactura = c.IdFactura ?? 0,
                        Cliente = cl.Nombre ?? string.Empty,
                        FechaEmision = c.FechaEmision ?? DateTime.MinValue,
                        FechaVencimiento = c.FechaVencimiento ?? DateTime.MinValue,
                        SaldoPendiente = c.SaldoPendiente ?? 0
                    })
                .OrderByDescending(c => c.SaldoPendiente)
                .ToListAsync();

            var productosTerminados = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.CantidadDisponible > 0)
                .Join(_productoRepo.GetQueryable().Where(p => p.Estado == 1),
                    l => l.IdProducto,
                    p => p.Id,
                    (l, p) => new BalanceProductoItemDTO
                    {
                        IdProducto = p.Id,
                        NombreProducto = p.Nombre ?? string.Empty,
                        CantidadDisponible = l.CantidadDisponible,
                        ValorUnitario = p.VentaPorPeso ? l.PrecioKilo : l.PrecioUnitario,
                        ValorTotal = l.CantidadDisponible * (p.VentaPorPeso ? l.PrecioKilo : l.PrecioUnitario)
                    })
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            var materiaPrima = await _loteMPRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.CantidadDisponible > 0)
                .Join(_materiaPrimaRepo.GetQueryable().Where(m => m.Estado == 1),
                    l => l.IdMateria,
                    m => m.Id,
                    (l, m) => new BalanceMateriaPrimaItemDTO
                    {
                        IdMateriaPrima = m.Id,
                        NombreMateriaPrima = m.Nombre ?? string.Empty,
                        CantidadDisponible = l.CantidadDisponible,
                        ValorUnitario = l.CostoUnitario,
                        ValorTotal = l.CantidadDisponible * l.CostoUnitario
                    })
                .OrderBy(m => m.NombreMateriaPrima)
                .ToListAsync();

            var cuentasPorCobrarTotal = cxcItems.Sum(c => c.SaldoPendiente);
            var inventarioProductosTotal = productosTerminados.Sum(p => p.ValorTotal);
            var inventarioMateriaPrimaTotal = materiaPrima.Sum(m => m.ValorTotal);
            var pasivosTotal = await _pasivosRepo.GetQueryable()
                .Where(p => p.Estado == 1)
                .SumAsync(p => p.Monto);
            var activosTotal = cuentasPorCobrarTotal + inventarioProductosTotal + inventarioMateriaPrimaTotal;

            return new BalanceMinimoReporteDTO
            {
                FechaGeneracion = DateTime.Now,
                CuentasPorCobrarTotal = cuentasPorCobrarTotal,
                InventarioProductosTotal = inventarioProductosTotal,
                InventarioMateriaPrimaTotal = inventarioMateriaPrimaTotal,
                ActivosTotal = activosTotal,
                PasivosTotal = pasivosTotal,
                PatrimonioTotal = activosTotal - pasivosTotal,
                PasivosNota = "Los pasivos se calculan desde la tabla pasivos con registros activos.",
                CuentasPorCobrar = cxcItems,
                ProductosTerminados = productosTerminados,
                MateriaPrima = materiaPrima
            };
        }

        // ──────────────────────────────────────────────
        // T-06: Ventas
        // ──────────────────────────────────────────────

        public async Task<VentasReporteDTO> GetVentasAsync(DateTime desde, DateTime hasta,
            Guid? idCliente, int? idProducto)
        {
            var facturasQuery = _facturaRepo.GetQueryable()
                .Where(f => f.Estado == 1 && f.Fecha.HasValue && f.Fecha.Value.Date >= desde.Date && f.Fecha.Value.Date <= hasta.Date);

            // Optional: filter by client
            if (idCliente.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.IdCliente == idCliente.Value);
            }

            // Optional: filter by product (via DetalleFactura)
            if (idProducto.HasValue)
            {
                var facturaIdsConProducto = _detalleRepo.GetQueryable()
                    .Where(d => d.IdProducto == idProducto.Value)
                    .Select(d => d.IdFactura)
                    .Distinct();

                facturasQuery = facturasQuery.Where(f => facturaIdsConProducto.Contains(f.Id));
            }

            var clientesQuery = _clienteRepo.GetQueryable();
            var tiposQuery = _tipoFacturaRepo.GetQueryable();

            var resultado = await facturasQuery
                .Join(clientesQuery,
                    f => f.IdCliente,
                    c => (Guid?)c.IdCliente,
                    (f, c) => new { f, ClienteNombre = c.Nombre ?? string.Empty })
                .Join(tiposQuery,
                    x => x.f.TipoFactura,
                    t => (int?)t.Id,
                    (x, t) => new VentaItemDTO
                    {
                        IdFactura = x.f.Id,
                        Fecha = x.f.Fecha ?? DateTime.MinValue,
                        Cliente = x.ClienteNombre,
                        Total = x.f.Total ?? 0,
                        Tipo = t.Nombre ?? string.Empty
                    })
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return new VentasReporteDTO
            {
                Resultados = resultado,
                TotalSum = resultado.Sum(v => v.Total),
                TotalCount = resultado.Count
            };
        }

        // ──────────────────────────────────────────────
        // T-04: CxC Aging
        // ──────────────────────────────────────────────

        public async Task<CxcAgingReporteDTO> GetCxcAgingAsync(Guid? idCliente)
        {
            var now = DateTime.Now;
            var cxcQuery = _cxcRepo.GetQueryable()
                .Where(c => c.SaldoPendiente > 0 && c.Estado == 1);

            if (idCliente.HasValue)
            {
                cxcQuery = cxcQuery.Where(c => c.IdCliente == idCliente.Value);
            }

            var clientesQuery = _clienteRepo.GetQueryable();

            var items = await cxcQuery
                .Join(clientesQuery,
                    c => c.IdCliente,
                    cl => (Guid?)cl.IdCliente,
                    (c, cl) => new
                    {
                        c.Id,
                        c.IdFactura,
                        ClienteNombre = cl.Nombre ?? string.Empty,
                        c.IdCliente,
                        c.FechaEmision,
                        c.FechaVencimiento,
                        c.SaldoPendiente
                    })
                .ToListAsync();

            // Group into buckets in memory (days past due needs C# computation)
            var buckets = new List<CxcBucketDTO>
            {
                new CxcBucketDTO { Key = "corriente" },
                new CxcBucketDTO { Key = "30_60" },
                new CxcBucketDTO { Key = "60_90" },
                new CxcBucketDTO { Key = "90_mas" }
            };

            foreach (var item in items)
            {
                var fechaVenc = item.FechaVencimiento ?? now;
                var diasVencidos = (now > fechaVenc) ? (int)(now - fechaVenc).TotalDays : 0;

                string bucketKey;
                if (diasVencidos <= 30)
                    bucketKey = "corriente";
                else if (diasVencidos <= 60)
                    bucketKey = "30_60";
                else if (diasVencidos <= 90)
                    bucketKey = "60_90";
                else
                    bucketKey = "90_mas";

                var bucket = buckets.First(b => b.Key == bucketKey);
                bucket.Count++;
                bucket.Total += item.SaldoPendiente ?? 0;
                bucket.Items.Add(new CxcItemDTO
                {
                    IdFactura = item.IdFactura ?? 0,
                    Cliente = item.ClienteNombre,
                    FechaEmision = item.FechaEmision ?? DateTime.MinValue,
                    FechaVencimiento = fechaVenc,
                    SaldoPendiente = item.SaldoPendiente ?? 0
                });
            }

            // Remove empty buckets
            buckets = buckets.Where(b => b.Count > 0).ToList();

            return new CxcAgingReporteDTO
            {
                Buckets = buckets,
                TotalGeneral = buckets.Sum(b => b.Total)
            };
        }

        // ──────────────────────────────────────────────
        // T-05: Inventario
        // ──────────────────────────────────────────────

        public async Task<InventarioReporteDTO> GetInventarioAsync(int? idCategoria, int stockThreshold = 10)
        {
            var now = DateTime.Now;

            var productosQuery = _productoRepo.GetQueryable().Where(p => p.Estado == 1);
            if (idCategoria.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.IdCategoria == idCategoria.Value);
            }

            var categoriasQuery = _categoriaRepo.GetQueryable();

            // Stock actual por producto (suma de CantidadDisponible de lotes activos)
            var stockPorProducto = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1)
                .GroupBy(l => l.IdProducto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    StockActual = g.Sum(l => l.CantidadDisponible)
                })
                .ToListAsync();

            var stockDict = stockPorProducto.ToDictionary(s => s.IdProducto, s => s.StockActual);

            // Productos con stock
            var productos = await productosQuery
                .Join(categoriasQuery,
                    p => p.IdCategoria,
                    c => c.Id,
                    (p, c) => new InventarioItemDTO
                    {
                        IdProducto = p.Id,
                        NombreProducto = p.Nombre ?? string.Empty,
                        IdCategoria = p.IdCategoria,
                        Categoria = c.Nombre ?? string.Empty,
                        StockActual = 0, // will fill below
                        StockBajo = false
                    })
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            foreach (var prod in productos)
            {
                prod.StockActual = stockDict.GetValueOrDefault(prod.IdProducto, 0);
                prod.StockBajo = prod.StockActual < stockThreshold;
            }

            // Lotes próximos a vencer (within next 30 days, not yet expired)
            var lotesProximos = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1
                    && l.FechaVencimiento > now
                    && l.FechaVencimiento <= now.AddDays(30))
                .Join(productosQuery,
                    l => l.IdProducto,
                    p => p.Id,
                    (l, p) => new LoteProximoVencerDTO
                    {
                        IdLote = l.Id,
                        Producto = p.Nombre ?? string.Empty,
                        Cantidad = l.CantidadDisponible,
                        FechaVencimiento = l.FechaVencimiento
                    })
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();

            return new InventarioReporteDTO
            {
                Productos = productos,
                LotesProximosVencer = lotesProximos
            };
        }

        // ──────────────────────────────────────────────
        // T-07: Cliente
        // ──────────────────────────────────────────────

        public async Task<ClienteReporteDTO?> GetClienteAsync(Guid idCliente)
        {
            var cliente = await _clienteRepo.FindByIdAsync(idCliente);
            if (cliente == null)
                return null;

            var facturas = await _facturaRepo.GetQueryable()
                .Where(f => f.IdCliente == idCliente && f.Estado == 1)
                .Join(_tipoFacturaRepo.GetQueryable(),
                    f => f.TipoFactura,
                    t => (int?)t.Id,
                    (f, t) => new FacturaClienteDTO
                    {
                        IdFactura = f.Id,
                        Fecha = f.Fecha ?? DateTime.MinValue,
                        Tipo = t.Nombre ?? string.Empty,
                        Total = f.Total ?? 0
                    })
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            var cxcSaldo = await _cxcRepo.GetQueryable()
                .Where(c => c.IdCliente == idCliente && c.SaldoPendiente > 0 && c.Estado == 1)
                .SumAsync(c => c.SaldoPendiente ?? 0);

            return new ClienteReporteDTO
            {
                IdCliente = cliente.IdCliente,
                ClienteNombre = cliente.Nombre ?? string.Empty,
                ClienteDocumento = cliente.Documento ?? string.Empty,
                Facturas = facturas,
                TotalGastado = facturas.Sum(f => f.Total),
                CantidadFacturas = facturas.Count,
                UltimaCompra = facturas.FirstOrDefault()?.Fecha,
                CxcSaldoPendiente = cxcSaldo
            };
        }

        // ──────────────────────────────────────────────
        // T-08: Movimientos
        // ──────────────────────────────────────────────

        public async Task<MovimientosReporteDTO> GetMovimientosAsync(DateTime desde, DateTime hasta,
            int? idProducto, int? tipoMovimiento)
        {
            var movimientosQuery = _movimientoRepo.GetQueryable()
                .Where(m => m.FechaMovimiento.Date >= desde.Date && m.FechaMovimiento.Date <= hasta.Date);

            if (tipoMovimiento.HasValue)
            {
                movimientosQuery = movimientosQuery.Where(m => m.TipoMovimiento == tipoMovimiento.Value);
            }

            // If filtering by product, go through LotesProductos to find matching lote IDs
            if (idProducto.HasValue)
            {
                var loteIds = _loteRepo.GetQueryable()
                    .Where(l => l.IdProducto == idProducto.Value)
                    .Select(l => (int?)l.Id);

                movimientosQuery = movimientosQuery.Where(m => loteIds.Contains(m.IdLoteProducto));
            }

            // Load raw movement data sorted desc
            var rawMovimientos = await movimientosQuery
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new
                {
                    m.Id,
                    m.FechaMovimiento,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.IdLoteProducto,
                    m.Observacion
                })
                .ToListAsync();

            // Collect distinct lote IDs and load related data
            var loteIdsList = rawMovimientos
                .Where(m => m.IdLoteProducto.HasValue)
                .Select(m => m.IdLoteProducto!.Value)
                .Distinct()
                .ToList();

            var lotes = await _loteRepo.GetQueryable()
                .Where(l => loteIdsList.Contains(l.Id))
                .Select(l => new { l.Id, l.IdProducto })
                .ToListAsync();

            var productoIds = lotes.Select(l => l.IdProducto).Distinct().ToList();
            var productos = await _productoRepo.GetQueryable()
                .Where(p => productoIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Nombre })
                .ToListAsync();

            var loteDict = lotes.ToDictionary(l => l.Id, l => l.IdProducto);
            var prodDict = productos.ToDictionary(p => p.Id, p => p.Nombre ?? "Producto eliminado");

            var movimientos = rawMovimientos.Select(m =>
            {
                var idProd = m.IdLoteProducto.HasValue && loteDict.ContainsKey(m.IdLoteProducto.Value)
                    ? loteDict[m.IdLoteProducto.Value]
                    : (int?)null;

                var nombreProducto = idProd.HasValue && prodDict.ContainsKey(idProd.Value)
                    ? prodDict[idProd.Value]
                    : "Producto eliminado";

                var tipoNombre = m.TipoMovimiento switch
                {
                    1 => "Entrada",
                    2 => "Venta",
                    3 => "Ajuste",
                    4 => "Devolución",
                    5 => "Vencimiento",
                    _ => "Desconocido"
                };

                return new MovimientoItemDTO
                {
                    Fecha = m.FechaMovimiento,
                    Producto = nombreProducto,
                    Tipo = tipoNombre,
                    Cantidad = m.Cantidad,
                    Lote = m.IdLoteProducto?.ToString() ?? string.Empty,
                    Observacion = m.Observacion
                };
            }).ToList();

            return new MovimientosReporteDTO
            {
                Movimientos = movimientos
            };
        }

        // ──────────────────────────────────────────────
        // Movimientos del Día
        // ──────────────────────────────────────────────

        public async Task<MovimientosDiariosDTO> GetMovimientosDiariosAsync()
        {
            var today = DateTime.Now.Date;

            // ── Facturación del día ──
            var facturasHoy = await _facturaRepo.GetQueryable()
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date == today && f.Estado == 1)
                .ToListAsync();

            var contado = facturasHoy.Count(f => f.TipoFactura == 1);
            var credito = facturasHoy.Count(f => f.TipoFactura == 2);

            // ── Movimientos MP del día ──
            var movMP = await _movMPRepo.GetQueryable()
                .Where(m => m.Fecha.Date == today)
                .GroupBy(m => m.IdTipoMovimiento)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            // ── Movimientos Productos del día ──
            var movProd = await _movProdRepo.GetQueryable()
                .Where(m => m.FechaMovimiento.Date == today)
                .GroupBy(m => m.TipoMovimiento)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            // ── CxC ──
            var cxcPendientes = await _cxcRepo.GetQueryable()
                .Where(c => c.Estado == 1 && c.SaldoPendiente.HasValue && c.SaldoPendiente > 0)
                .ToListAsync();

            var cxcVencidas = cxcPendientes
                .Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value.Date < today)
                .ToList();

            // ── Alertas ──
            var alertas = new List<string>();

            var proximosVencer = await _loteMPRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= today.AddDays(7) && l.CantidadDisponible > 0)
                .CountAsync();

            if (proximosVencer > 0)
                alertas.Add($"{proximosVencer} lote(s) de materia prima próximos a vencer (7 días)");

            var productosVencer = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= today.AddDays(30) && l.CantidadDisponible > 0)
                .CountAsync();

            if (productosVencer > 0)
                alertas.Add($"{productosVencer} lote(s) de productos próximos a vencer (30 días)");

            if (cxcVencidas.Count > 0)
                alertas.Add($"{cxcVencidas.Count} cuenta(s) por cobrar vencidas");

            var stockBajo = await _loteRepo.GetQueryable()
                .Where(l => l.CantidadDisponible <= 5 && l.Estado == 1)
                .CountAsync();

            if (stockBajo > 0)
                alertas.Add($"{stockBajo} lote(s) de productos con stock bajo");

            return new MovimientosDiariosDTO
            {
                Fecha = today.ToString("dd/MM/yyyy"),
                Facturas = new ResumenFacturasDTO
                {
                    Cantidad = facturasHoy.Count,
                    Contado = contado,
                    Credito = credito,
                    Total = facturasHoy.Sum(f => f.Total ?? 0)
                },
                MateriaPrima = new ResumenMovimientosMPDTO
                {
                    Entradas = movMP.Where(m => m.Tipo == 1).Sum(m => m.Count),
                    Consumos = movMP.Where(m => m.Tipo == 2).Sum(m => m.Count),
                    Ajustes = movMP.Where(m => m.Tipo == 3).Sum(m => m.Count),
                    Devoluciones = movMP.Where(m => m.Tipo == 4).Sum(m => m.Count),
                    Vencimientos = movMP.Where(m => m.Tipo == 5).Sum(m => m.Count)
                },
                Productos = new ResumenMovimientosProductosDTO
                {
                    Entradas = movProd.Where(m => m.Tipo == 1).Sum(m => m.Count),
                    Ventas = movProd.Where(m => m.Tipo == 2).Sum(m => m.Count),
                    Ajustes = movProd.Where(m => m.Tipo == 3).Sum(m => m.Count),
                    Devoluciones = movProd.Where(m => m.Tipo == 4).Sum(m => m.Count),
                    Vencimientos = movProd.Where(m => m.Tipo == 5).Sum(m => m.Count)
                },
                CuentasCobrar = new ResumenCxcDiarioDTO
                {
                    Pendientes = cxcPendientes.Count,
                    Vencidas = cxcVencidas.Count,
                    TotalPendiente = cxcPendientes.Sum(c => c.SaldoPendiente ?? 0),
                    TotalVencido = cxcVencidas.Sum(c => c.SaldoPendiente ?? 0)
                },
                Alertas = alertas
            };
        }
    }
}
