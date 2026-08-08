using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class LotesProductosServiceTests
    {
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesRepo = new();
        private readonly Mock<IGenericRepository<MovimientosProductosEntity, int>> _movimientosRepo = new();
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productosRepo = new();
        private readonly Mock<IGenericRepository<ProveedoresEntity, Guid>> _proveedoresRepo = new();
        private readonly Mock<IGenericRepository<MarcasEntity, int>> _marcasRepo = new();
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepo = new();
        private readonly Mock<IAuditoriaService> _auditoriaService = new();
        private readonly LotesProductosService _service;

        public LotesProductosServiceTests()
        {
            _service = new LotesProductosService(
                _lotesRepo.Object,
                _movimientosRepo.Object,
                _productosRepo.Object,
                _proveedoresRepo.Object,
                _marcasRepo.Object,
                _unidadMedidaRepo.Object,
                _auditoriaService.Object);

            _lotesRepo.Setup(r => r.UpdateAsync(It.IsAny<LotesProductosEntity>())).Returns(Task.CompletedTask);
            _lotesRepo.Setup(r => r.CreateAsync(It.IsAny<LotesProductosEntity>())).Returns(Task.CompletedTask);
            _movimientosRepo.Setup(r => r.CreateAsync(It.IsAny<MovimientosProductosEntity>())).Returns(Task.CompletedTask);
            _productosRepo.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProductosEntity { Id = 1, Nombre = "Producto", Estado = 1 });
        }

        [Fact]
        public async Task CrearLoteProducto_DebeUsarCantidadInicialComoCantidadDisponible()
        {
            var capturado = new LotesProductosEntity();
            _productosRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto",
                Estado = 1,
                PesoPorUnidad = 0.5m
            });

            _lotesRepo.Setup(r => r.CreateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaVencimiento = DateTime.Today.AddDays(10),
                CantidadUnidades = 8,
                PesoTotal = 4m,
                IdUnidadMedida = 1,
                PrecioUnitario = 10m,
                PrecioKilo = 20m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.CrearLoteProductoAsync(dto);

            capturado.CantidadInicial.Should().Be(capturado.CantidadDisponible);
            capturado.PesoDisponible.Should().Be(4m);
        }

        [Fact]
        public async Task ActualizarLoteProducto_NoDebeReemplazarFechaEntradaNiCantidadInicial()
        {
            var fechaEntradaOriginal = new DateTime(2026, 1, 10);
            var lote = new LotesProductosEntity
            {
                Id = 15,
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaEntrada = fechaEntradaOriginal,
                FechaVencimiento = new DateTime(2026, 2, 10),
                CantidadUnidades = 10,
                PesoTotal = 10m,
                IdUnidadMedida = 2,
                PrecioUnitario = 12m,
                PrecioKilo = 12m,
                PrecioTotal = 120m,
                IdMarca = 1,
                CantidadInicial = 10m,
                CantidadDisponible = 10m,
                PesoDisponible = 10m,
                Estado = 1
            };

            LotesProductosEntity? capturado = null;

            _lotesRepo.Setup(r => r.FindByIdAsync(15)).ReturnsAsync(lote);
            _lotesRepo.Setup(r => r.UpdateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = lote.IdProveedor,
                FechaVencimiento = DateTime.Today.AddDays(-2),
                CantidadUnidades = 12,
                PesoTotal = 12m,
                IdUnidadMedida = 2,
                PrecioUnitario = 15m,
                PrecioKilo = 15m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.ActualizarLoteProducto(15, dto);

            capturado.Should().NotBeNull();
            capturado!.FechaEntrada.Should().Be(fechaEntradaOriginal);
            capturado.CantidadInicial.Should().Be(10m);
            capturado.FechaVencimiento.Should().Be(dto.FechaVencimiento);
        }

        [Fact]
        public async Task ActualizarLoteProducto_DebeAjustarDisponiblesPorDiferenciaRealDelLote()
        {
            var lote = new LotesProductosEntity
            {
                Id = 15,
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaEntrada = new DateTime(2026, 1, 10),
                FechaVencimiento = new DateTime(2026, 2, 10),
                CantidadUnidades = 1000,
                PesoTotal = 10000m,
                IdUnidadMedida = 1,
                PrecioUnitario = 12m,
                PrecioKilo = 12m,
                PrecioTotal = 12000m,
                IdMarca = 1,
                CantidadInicial = 1000m,
                CantidadDisponible = 800m,
                PesoDisponible = 8000m,
                Estado = 1
            };

            LotesProductosEntity? capturado = null;

            _lotesRepo.Setup(r => r.FindByIdAsync(15)).ReturnsAsync(lote);
            _lotesRepo.Setup(r => r.UpdateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = lote.IdProveedor,
                FechaVencimiento = new DateTime(2026, 3, 10),
                CantidadUnidades = 1200,
                PesoTotal = 12000m,
                IdUnidadMedida = 1,
                PrecioUnitario = 15m,
                PrecioKilo = 15m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.ActualizarLoteProducto(15, dto);

            capturado.Should().NotBeNull();
            capturado!.CantidadDisponible.Should().Be(1000m);
            capturado.PesoDisponible.Should().Be(10000m);
        }

        [Fact]
        public async Task LotesProductosDTOValidator_CreateMode_RechazaVencimientoDeHoy()
        {
            var validator = new LotesProductosDTOValidator();
            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaVencimiento = DateTime.Today,
                CantidadUnidades = 1,
                PesoTotal = 1m,
                IdUnidadMedida = 2,
                PrecioUnitario = 1m,
                PrecioKilo = 1m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            var result = await validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LotesProductosDTO.FechaVencimiento));
        }
    }
}
