using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Reportes;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
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
        private readonly IGenericRepository<ActivosEntity, int> _activosRepo;
        private readonly IGenericRepository<PatrimonioEntity, int> _patrimonioRepo;
        private readonly IGenericRepository<CajaAperturaEntity, int> _cajaAperturaRepo;
        private readonly IGenericRepository<CajaMovimientoEntity, int> _cajaMovimientoRepo;
        private readonly ICuentasPagarService _cxpService;

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
            IGenericRepository<PasivosEntity, int> pasivosRepo,
            IGenericRepository<ActivosEntity, int> activosRepo,
            IGenericRepository<PatrimonioEntity, int> patrimonioRepo,
            IGenericRepository<CajaAperturaEntity, int> cajaAperturaRepo,
            IGenericRepository<CajaMovimientoEntity, int> cajaMovimientoRepo,
            ICuentasPagarService cxpService)
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
            _activosRepo = activosRepo;
            _patrimonioRepo = patrimonioRepo;
            _cajaAperturaRepo = cajaAperturaRepo;
            _cajaMovimientoRepo = cajaMovimientoRepo;
            _cxpService = cxpService;
        }


        public async Task<DashboardDTO> GetDashboardAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;

            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var facturasQuery = _facturaRepo.GetQueryable().Where(f => f.Estado == 1);

            var ventasHoy = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date == today)
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

            var ventasSemana = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date >= startOfWeek && f.Fecha.Value.Date < startOfWeek.AddDays(7))
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

            var ventasMes = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Year == today.Year && f.Fecha.Value.Month == today.Month)
                .GroupBy(_ => 1)
                .Select(g => new VentasPeriodoDTO
                {
                    Total = g.Sum(f => f.Total ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync() ?? new VentasPeriodoDTO();

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

            var alertas = new List<string>();

            var proximosVencerMP = await _loteMPRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= today.AddDays(7) && l.CantidadDisponible > 0)
                .CountAsync();

            if (proximosVencerMP > 0)
                alertas.Add($"{proximosVencerMP} lote(s) de materia prima próximos a vencer (7 días)");

            var productosVencer = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= today.AddDays(30) && l.CantidadDisponible > 0)
                .CountAsync();

            if (productosVencer > 0)
                alertas.Add($"{productosVencer} lote(s) de productos próximos a vencer (30 días)");

            var cxcVencidasCount = await _cxcRepo.GetQueryable()
                .Where(c => c.Estado == 1 && c.SaldoPendiente > 0
                    && c.FechaVencimiento.HasValue && c.FechaVencimiento.Value.Date < today)
                .CountAsync();

            if (cxcVencidasCount > 0)
                alertas.Add($"{cxcVencidasCount} cuenta(s) por cobrar vencidas");

            var stockBajoCount = stockBajo.Count;
            if (stockBajoCount > 0)
                alertas.Add($"{stockBajoCount} producto(s) con stock bajo");

            var facturasHoy = await facturasQuery
                .Where(f => f.Fecha.HasValue && f.Fecha.Value.Date == today)
                .ToListAsync();

            var ventasContado = new VentasPeriodoDTO
            {
                Total = facturasHoy.Where(f => f.TipoFactura == 1).Sum(f => f.Total ?? 0),
                Count = facturasHoy.Count(f => f.TipoFactura == 1)
            };

            var ventasCredito = new VentasPeriodoDTO
            {
                Total = facturasHoy.Where(f => f.TipoFactura == 2).Sum(f => f.Total ?? 0),
                Count = facturasHoy.Count(f => f.TipoFactura == 2)
            };

            var stockBajoProductIds = stockBajo.Select(s => s.IdProducto).ToList();
            var productosConCategoria = await _productoRepo.GetQueryable()
                .Where(p => stockBajoProductIds.Contains(p.Id))
                .Select(p => new { p.Id, p.IdCategoria })
                .ToListAsync();

            var categorias = await _categoriaRepo.GetQueryable()
                .Select(c => new { c.Id, c.Nombre })
                .ToListAsync();

            var categoriaDict = categorias.ToDictionary(c => c.Id, c => c.Nombre ?? "Sin categoría");
            var prodCategoriaDict = productosConCategoria.ToDictionary(p => p.Id, p => p.IdCategoria);

            var stockBajoPorCategoria = stockBajo
                .GroupBy(s => prodCategoriaDict.GetValueOrDefault(s.IdProducto, 0))
                .Select(g => new StockBajoCategoriaDTO
                {
                    IdCategoria = g.Key,
                    CategoriaNombre = categoriaDict.GetValueOrDefault(g.Key, "Sin categoría"),
                    ProductosBajos = g.Count()
                })
                .OrderByDescending(s => s.ProductosBajos)
                .ToList();

            return new DashboardDTO
            {
                VentasHoy = ventasHoy,
                VentasSemana = ventasSemana,
                VentasMes = ventasMes,
                TopProductos = topProductos,
                CxcPendiente = cxcPendiente,
                CxcVencido = cxcVencido,
                StockBajo = stockBajo,
                UltimasFacturas = ultimasFacturas,
                Alertas = alertas,
                VentasContado = ventasContado,
                VentasCredito = ventasCredito,
                StockBajoPorCategoria = stockBajoPorCategoria
            };
        }


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

            var activosItems = await _activosRepo.GetQueryable()
                .Where(a => a.Estado == 1)
                .Select(a => new BalanceActivoItemDTO
                {
                    IdActivo = a.Id,
                    Nombre = a.Nombre ?? string.Empty,
                    Descripcion = a.Descripcion,
                    Monto = a.Monto
                })
                .ToListAsync();

            var activosDirectoTotal = activosItems.Sum(a => a.Monto);

            var aperturas = await _cajaAperturaRepo.GetAllAsync();
            var efectivoItems = new List<BalanceEfectivoItemDTO>();
            decimal efectivoTotal = 0;

            foreach (var apertura in aperturas)
            {
                decimal totalIngresos = apertura.TotalIngresos ?? 0;
                decimal totalEgresos = apertura.TotalEgresos ?? 0;

                if (!apertura.TotalIngresos.HasValue || !apertura.TotalEgresos.HasValue)
                {
                    var movimientos = _cajaMovimientoRepo.GetByFilter(m => m.IdApertura == apertura.Id);
                    totalIngresos = movimientos.Where(m => m.TipoMovimiento == 1).Sum(m => m.Monto);
                    totalEgresos = movimientos.Where(m => m.TipoMovimiento == 2).Sum(m => m.Monto);
                }

                decimal saldo = apertura.MontoInicial + totalIngresos - totalEgresos;
                efectivoTotal += saldo;

                efectivoItems.Add(new BalanceEfectivoItemDTO
                {
                    IdApertura = apertura.Id,
                    FechaApertura = apertura.FechaApertura,
                    MontoInicial = apertura.MontoInicial,
                    TotalIngresos = totalIngresos,
                    TotalEgresos = totalEgresos,
                    Saldo = saldo
                });
            }

            var pasivosTotal = await _pasivosRepo.GetQueryable()
                .Where(p => p.Estado == 1)
                .SumAsync(p => p.Monto);

            var cxpEntities = await _cxpService.ObtenerTodosAsync(1);
            var cxpItems = cxpEntities.Where(c => c.SaldoPendiente > 0).ToList();
            var cuentasPagarItems = cxpItems.Select(c => new BalanceCuentasPagarItemDTO
            {
                IdCuentaPagar = c.IdCuentaPagar,
                Proveedor = c.ProveedorNombre ?? "Proveedor eliminado",
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                FechaVencimiento = c.FechaVencimiento
            }).OrderByDescending(c => c.SaldoPendiente).ToList();
            decimal cuentasPagarTotal = cxpItems.Sum(c => c.SaldoPendiente);
            pasivosTotal += cuentasPagarTotal;

            var patrimoniosItems = await _patrimonioRepo.GetQueryable()
                .Where(p => p.Estado == 1)
                .Select(p => new BalancePatrimonioItemDTO
                {
                    IdPatrimonio = p.Id,
                    Nombre = p.Nombre ?? string.Empty,
                    Descripcion = p.Descripcion,
                    Monto = p.Monto
                })
                .ToListAsync();

            var patrimonioCapitalTotal = patrimoniosItems.Sum(p => p.Monto);

            var activosTotal = cuentasPorCobrarTotal + inventarioProductosTotal + inventarioMateriaPrimaTotal + activosDirectoTotal + efectivoTotal;
            var patrimonioComputed = activosTotal - pasivosTotal;

            return new BalanceMinimoReporteDTO
            {
                FechaGeneracion = DateTime.Now,
                CuentasPorCobrarTotal = cuentasPorCobrarTotal,
                InventarioProductosTotal = inventarioProductosTotal,
                InventarioMateriaPrimaTotal = inventarioMateriaPrimaTotal,
                EfectivoEquivalenteTotal = efectivoTotal,
                ActivosTotal = activosTotal,
                PasivosTotal = pasivosTotal,
                CuentasPagarTotal = cuentasPagarTotal,
                PatrimonioComputed = patrimonioComputed,
                PatrimonioTotal = patrimonioCapitalTotal,
                PatrimonioCapitalTotal = patrimonioCapitalTotal,
                ActivosNota = "Los activos incluyen cuentas por cobrar, inventario de productos, materia prima, registros de la tabla activos y efectivo (caja).",
                PasivosNota = "Los pasivos incluyen registros de la tabla pasivos y cuentas por pagar pendientes.",
                PatrimonioNota = "El patrimonio se calcula como Activos - Pasivos. Los aportes de capital son registros manuales de la tabla patrimonio.",
                CuentasPorCobrar = cxcItems,
                ProductosTerminados = productosTerminados,
                MateriaPrima = materiaPrima,
                Activos = activosItems,
                EfectivoItems = efectivoItems,
                CuentasPagarItems = cuentasPagarItems,
                Patrimonios = patrimoniosItems,
                PatrimonioCapitalItems = patrimoniosItems
            };
        }


        public async Task<VentasReporteDTO> GetVentasAsync(DateTime desde, DateTime hasta,
            Guid? idCliente, int? idProducto)
        {
            var desdeDate = desde.Date;
            var hastaDate = hasta.Date.AddDays(1).AddTicks(-1);

            var facturasQuery = _facturaRepo.GetQueryable()
                .Where(f => f.Estado == 1 && f.Fecha.HasValue && f.Fecha.Value >= desdeDate && f.Fecha.Value <= hastaDate);

            if (idCliente.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.IdCliente == idCliente.Value);
            }

            if (idProducto.HasValue)
            {
                var facturaIdsConProducto = _detalleRepo.GetQueryable()
                    .Where(d => d.IdProducto == idProducto.Value)
                    .Select(d => d.IdFactura)
                    .Distinct();

                facturasQuery = facturasQuery.Where(f => facturaIdsConProducto.Contains(f.Id));
            }

            var clientesQuery = _clienteRepo.GetQueryable();
            var tiposDict = await _tipoFacturaRepo.GetQueryable()
                .ToDictionaryAsync(t => t.Id, t => t.Nombre);

            var rawResult = await facturasQuery
                .Join(clientesQuery,
                    f => f.IdCliente,
                    c => (Guid?)c.IdCliente,
                    (f, c) => new { f, ClienteNombre = c.Nombre ?? string.Empty })
                .OrderByDescending(x => x.f.Fecha)
                .ToListAsync();

            var resultado = rawResult.Select(x => new VentaItemDTO
            {
                IdFactura = x.f.Id,
                Fecha = x.f.Fecha ?? DateTime.MinValue,
                Cliente = x.ClienteNombre,
                Total = x.f.Total ?? 0,
                Tipo = x.f.TipoFactura.HasValue && tiposDict.ContainsKey(x.f.TipoFactura.Value)
                    ? tiposDict[x.f.TipoFactura.Value] ?? "Desconocido"
                    : "Desconocido"
            }).ToList();

            return new VentasReporteDTO
            {
                Resultados = resultado,
                TotalSum = resultado.Sum(v => v.Total),
                TotalCount = resultado.Count
            };
        }


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

            var buckets = new List<CxcBucketDTO>
            {
                new CxcBucketDTO { Key = "corriente", Nombre = "Corriente (≤30 días)" },
                new CxcBucketDTO { Key = "30_60", Nombre = "30-60 días" },
                new CxcBucketDTO { Key = "60_90", Nombre = "60-90 días" },
                new CxcBucketDTO { Key = "90_mas", Nombre = "Más de 90 días" }
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

            buckets = buckets.Where(b => b.Count > 0).ToList();

            return new CxcAgingReporteDTO
            {
                Buckets = buckets,
                TotalGeneral = buckets.Sum(b => b.Total)
            };
        }


        public async Task<InventarioReporteDTO> GetInventarioAsync(int? idCategoria, int stockThreshold = 10)
        {
            var now = DateTime.Now;

            var productosQuery = _productoRepo.GetQueryable().Where(p => p.Estado == 1);
            if (idCategoria.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.IdCategoria == idCategoria.Value);
            }

            var categoriasQuery = _categoriaRepo.GetQueryable();

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
                        StockActual = 0,
                        StockBajo = false
                    })
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            foreach (var prod in productos)
            {
                prod.StockActual = stockDict.GetValueOrDefault(prod.IdProducto, 0);
                prod.StockBajo = prod.StockActual < stockThreshold;
            }

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


        public async Task<ClienteReporteDTO?> GetClienteAsync(Guid idCliente)
        {
            var cliente = await _clienteRepo.FindByIdAsync(idCliente);
            if (cliente == null)
                return null;

            var tiposDict = await _tipoFacturaRepo.GetQueryable()
                .ToDictionaryAsync(t => t.Id, t => t.Nombre);

            var rawFacturas = await _facturaRepo.GetQueryable()
                .Where(f => f.IdCliente == idCliente && f.Estado == 1)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            var facturas = rawFacturas.Select(f => new FacturaClienteDTO
            {
                IdFactura = f.Id,
                Fecha = f.Fecha ?? DateTime.MinValue,
                Tipo = f.TipoFactura.HasValue && tiposDict.ContainsKey(f.TipoFactura.Value)
                    ? tiposDict[f.TipoFactura.Value] ?? "Desconocido"
                    : "Desconocido",
                Total = f.Total ?? 0
            }).ToList();

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


        public async Task<MovimientosReporteDTO> GetMovimientosAsync(DateTime desde, DateTime hasta,
            int? idProducto, int? tipoMovimiento)
        {
            var desdeDate = desde.Date;
            var hastaDate = hasta.Date.AddDays(1).AddTicks(-1);

            var movimientosQuery = _movimientoRepo.GetQueryable()
                .Where(m => m.FechaMovimiento >= desdeDate && m.FechaMovimiento <= hastaDate);

            if (tipoMovimiento.HasValue)
            {
                movimientosQuery = movimientosQuery.Where(m => m.TipoMovimiento == tipoMovimiento.Value);
            }

            if (idProducto.HasValue)
            {
                var loteIds = _loteRepo.GetQueryable()
                    .Where(l => l.IdProducto == idProducto.Value)
                    .Select(l => (int?)l.Id);

                movimientosQuery = movimientosQuery.Where(m => loteIds.Contains(m.IdLoteProducto));
            }

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


        public async Task<MovimientosDiariosDTO> GetMovimientosDiariosAsync()
        {
            var todayStart = DateTime.Now.Date;
            var todayEnd = todayStart.AddDays(1);

            var facturasHoy = await _facturaRepo.GetQueryable()
                .Where(f => f.Fecha.HasValue && f.Fecha.Value >= todayStart && f.Fecha.Value < todayEnd && f.Estado == 1)
                .ToListAsync();

            var contado = facturasHoy.Count(f => f.TipoFactura == 1);
            var credito = facturasHoy.Count(f => f.TipoFactura == 2);

            var movMP = await _movMPRepo.GetQueryable()
                .Where(m => m.Fecha >= todayStart && m.Fecha < todayEnd)
                .GroupBy(m => m.IdTipoMovimiento)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            var movProd = await _movProdRepo.GetQueryable()
                .Where(m => m.FechaMovimiento >= todayStart && m.FechaMovimiento < todayEnd)
                .GroupBy(m => m.TipoMovimiento)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            var cxcPendientes = await _cxcRepo.GetQueryable()
                .Where(c => c.Estado == 1 && c.SaldoPendiente.HasValue && c.SaldoPendiente > 0)
                .ToListAsync();

            var cxcVencidas = cxcPendientes
                .Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < todayStart)
                .ToList();

            var alertas = new List<string>();

            var proximosVencer = await _loteMPRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= todayStart.AddDays(7) && l.CantidadDisponible > 0)
                .CountAsync();

            if (proximosVencer > 0)
                alertas.Add($"{proximosVencer} lote(s) de materia prima próximos a vencer (7 días)");

            var productosVencer = await _loteRepo.GetQueryable()
                .Where(l => l.Estado == 1 && l.FechaVencimiento <= todayStart.AddDays(30) && l.CantidadDisponible > 0)
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
                Fecha = todayStart.ToString("dd/MM/yyyy"),
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
