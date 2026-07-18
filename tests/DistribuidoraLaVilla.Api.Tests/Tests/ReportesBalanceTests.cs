using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Reportes;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Infrastructure.Data;
using DistribuidoraLaVilla.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class ReportesBalanceTests : IDisposable
    {
        private readonly DataContext _context;
        private readonly ReportesService _service;
        private readonly Mock<ICuentasPagarService> _cxpServiceMock;

        public ReportesBalanceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _cxpServiceMock = new Mock<ICuentasPagarService>();

            // Default: CxP service returns empty list (avoids NullReferenceException)
            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(It.IsAny<int?>()))
                .ReturnsAsync(new List<CuentasPagarDTO>());

            _service = new ReportesService(
                new GenericRepository<FacturaEntity, int>(_context),
                new GenericRepository<DetalleFacturaEntity, int>(_context),
                new GenericRepository<ProductosEntity, int>(_context),
                new GenericRepository<CuentasCobrarEntity, int>(_context),
                new GenericRepository<LotesProductosEntity, int>(_context),
                new GenericRepository<ClientesEntity, Guid>(_context),
                new GenericRepository<MateriaPrimaEntity, int>(_context),
                new GenericRepository<CategoriasProductosEntity, int>(_context),
                new GenericRepository<MovimientoEntity, int>(_context),
                new GenericRepository<TipoFacturaEntity, int>(_context),
                new GenericRepository<MovimientosMateriaPrimaEntity, int>(_context),
                new GenericRepository<MovimientosProductosEntity, int>(_context),
                new GenericRepository<LotesMateriaPrimaEntity, int>(_context),
                new GenericRepository<PasivosEntity, int>(_context),
                new GenericRepository<ActivosEntity, int>(_context),
                new GenericRepository<PatrimonioEntity, int>(_context),
                new GenericRepository<CajaAperturaEntity, int>(_context),
                new GenericRepository<CajaMovimientoEntity, int>(_context),
                _cxpServiceMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ── Seed helpers ──

        private static ClientesEntity CreateCliente(Guid? id = null) => new()
        {
            IdCliente = id ?? Guid.NewGuid(),
            Nombre = "Cliente Test",
            Estado = 1,
            FechaCreacion = DateTime.Now
        };

        // ── Efectivo tests ──

        [Fact]
        public async Task GetBalanceMinimo_EfectivoEquivalenteTotal_DeberiaSumarSaldoDeAperturas()
        {
            // Arrange — 2 aperturas with known totals
            _context.CajaAperturas.AddRange(
                new CajaAperturaEntity
                {
                    Id = 1, MontoInicial = 10000, TotalIngresos = 5000,
                    TotalEgresos = 2000, Estado = 1, FechaApertura = DateTime.Now.AddDays(-2),
                    IdUsuario = Guid.NewGuid()
                },
                new CajaAperturaEntity
                {
                    Id = 2, MontoInicial = 8000, TotalIngresos = 1000,
                    TotalEgresos = 500, Estado = 2, FechaApertura = DateTime.Now.AddDays(-1),
                    IdUsuario = Guid.NewGuid()
                });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — Apertura 1: 10000+5000-2000=13000, Apertura 2: 8000+1000-500=8500
            result.EfectivoEquivalenteTotal.Should().Be(21500m);
            result.EfectivoItems.Should().HaveCount(2);
            result.EfectivoItems.Should().Contain(e => e.Saldo == 13000m);
            result.EfectivoItems.Should().Contain(e => e.Saldo == 8500m);
        }

        [Fact]
        public async Task GetBalanceMinimo_EfectivoEquivalenteTotal_ConTotalesNull_DeberiaCalcularDesdeMovimientos()
        {
            // Arrange — apertura with NULL totals, need to compute from movements
            _context.CajaAperturas.Add(new CajaAperturaEntity
            {
                Id = 1,
                MontoInicial = 10000,
                TotalIngresos = null,
                TotalEgresos = null,
                Estado = 1,
                FechaApertura = DateTime.Now,
                IdUsuario = Guid.NewGuid()
            });

            _context.CajaMovimientos.AddRange(
                new CajaMovimientoEntity
                {
                    Id = 1, IdApertura = 1, TipoMovimiento = 1 /*ingreso*/,
                    Monto = 3000, Fecha = DateTime.Now, IdUsuario = Guid.NewGuid()
                },
                new CajaMovimientoEntity
                {
                    Id = 2, IdApertura = 1, TipoMovimiento = 1 /*ingreso*/,
                    Monto = 2000, Fecha = DateTime.Now, IdUsuario = Guid.NewGuid()
                },
                new CajaMovimientoEntity
                {
                    Id = 3, IdApertura = 1, TipoMovimiento = 2 /*egreso*/,
                    Monto = 1500, Fecha = DateTime.Now, IdUsuario = Guid.NewGuid()
                });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — 10000 + (3000+2000) - 1500 = 13500
            result.EfectivoEquivalenteTotal.Should().Be(13500m);
            result.EfectivoItems.Should().HaveCount(1);
            result.EfectivoItems[0].TotalIngresos.Should().Be(5000m);
            result.EfectivoItems[0].TotalEgresos.Should().Be(1500m);
            result.EfectivoItems[0].Saldo.Should().Be(13500m);
        }

        [Fact]
        public async Task GetBalanceMinimo_EfectivoEquivalenteTotal_SinAperturas_DeberiaSerCero()
        {
            // Arrange — no aperturas seeded

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert
            result.EfectivoEquivalenteTotal.Should().Be(0m);
            result.EfectivoItems.Should().BeEmpty();
        }

        // ── CuentasPagarTotal tests ──

        [Fact]
        public async Task GetBalanceMinimo_CuentasPagarTotal_DeberiaSumarSaldoPendiente()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>
                {
                    new() { IdCuentaPagar = 1, IdProveedor = proveedorId, ProveedorNombre = "Prov A",
                            Estado = 1, MontoTotal = 1000, SaldoPendiente = 800 },
                    new() { IdCuentaPagar = 2, IdProveedor = proveedorId, ProveedorNombre = "Prov B",
                            Estado = 1, MontoTotal = 500, SaldoPendiente = 500 }
                });

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert
            result.CuentasPagarTotal.Should().Be(1300m);
            result.CuentasPagarItems.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetBalanceMinimo_CuentasPagarTotal_CuandoNoHayCxPPendientes_DeberiaSerCero()
        {
            // Arrange — only paid CxP returned (saldo_pendiente = 0)
            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>
                {
                    new() { IdCuentaPagar = 1, IdProveedor = Guid.NewGuid(), ProveedorNombre = "Prov A",
                            Estado = 2 /*pagada*/, MontoTotal = 1000, SaldoPendiente = 0 }
                });

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — the filter `.Where(c => c.SaldoPendiente > 0)` excludes paid items
            result.CuentasPagarTotal.Should().Be(0m);
            result.CuentasPagarItems.Should().BeEmpty();
        }

        // ── PatrimonioComputed test ──

        [Fact]
        public async Task GetBalanceMinimo_PatrimonioComputed_DeberiaSerActivosMenosPasivos()
        {
            // Arrange — seed pasivos
            _context.Pasivos.Add(new PasivosEntity
            {
                Id = 1, Nombre = "Préstamo", Monto = 5000, Estado = 1,
                FechaCreacion = DateTime.Now
            });

            // No activos, no CxC, no inventory, no efectivo, no CxP
            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>());

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — ActivosTotal=0 (no CxC, no inventory, no efectivo, no activos directos)
            // PasivosTotal=5000 (pasivos manual)
            // PatrimonioComputed = 0 - 5000 = -5000
            result.PatrimonioComputed.Should().Be(-5000m);
            result.PasivosTotal.Should().Be(5000m);
            result.ActivosTotal.Should().Be(0m);
        }

        // ── PatrimonioCapitalTotal test ──

        [Fact]
        public async Task GetBalanceMinimo_PatrimonioCapitalTotal_DeberiaSumarEntradasManuales()
        {
            // Arrange
            _context.Patrimonio.AddRange(
                new PatrimonioEntity
                {
                    Id = 1, Nombre = "Aporte Inicial", Monto = 50000, Estado = 1,
                    FechaCreacion = DateTime.Now
                },
                new PatrimonioEntity
                {
                    Id = 2, Nombre = "Segundo Aporte", Monto = 25000, Estado = 1,
                    FechaCreacion = DateTime.Now
                },
                new PatrimonioEntity
                {
                    Id = 3, Nombre = "Aporte Cancelado", Monto = 10000, Estado = 3,
                    FechaCreacion = DateTime.Now
                });

            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>());

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — only Estado=1: 50000 + 25000 = 75000
            result.PatrimonioCapitalTotal.Should().Be(75000m);
            result.PatrimonioCapitalItems.Should().HaveCount(2);
        }

        // ── Activos included in ActivosTotal ──

        [Fact]
        public async Task GetBalanceMinimo_ActivosDirectos_DeberiaIncluirseEnActivosTotal()
        {
            // Arrange
            _context.Activos.AddRange(
                new ActivosEntity
                {
                    Id = 1, Nombre = "Equipo", Monto = 15000, Estado = 1,
                    FechaCreacion = DateTime.Now
                },
                new ActivosEntity
                {
                    Id = 2, Nombre = "Mueble", Monto = 3000, Estado = 1,
                    FechaCreacion = DateTime.Now
                });

            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>());

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — ActivosTotal should include activos directos: 15000 + 3000 = 18000
            result.ActivosTotal.Should().Be(18000m);
            result.Activos.Should().HaveCount(2);
        }

        // ── Pasivos include CxP ──

        [Fact]
        public async Task GetBalanceMinimo_Pasivos_DeberiaIncluirCxPYPasivosManuales()
        {
            // Arrange — manual pasivos
            _context.Pasivos.Add(new PasivosEntity
            {
                Id = 1, Nombre = "Préstamo", Monto = 10000, Estado = 1,
                FechaCreacion = DateTime.Now
            });

            // CxP with pending balance
            var proveedorId = Guid.NewGuid();
            _cxpServiceMock
                .Setup(s => s.ObtenerTodosAsync(1))
                .ReturnsAsync(new List<CuentasPagarDTO>
                {
                    new() { IdCuentaPagar = 1, IdProveedor = proveedorId, ProveedorNombre = "Prov",
                            Estado = 1, MontoTotal = 3000, SaldoPendiente = 2000 }
                });

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBalanceMinimoAsync();

            // Assert — PasivosTotal = 10000 (manual) + 2000 (CxP) = 12000
            result.PasivosTotal.Should().Be(12000m);
            result.CuentasPagarTotal.Should().Be(2000m);
        }
    }
}
