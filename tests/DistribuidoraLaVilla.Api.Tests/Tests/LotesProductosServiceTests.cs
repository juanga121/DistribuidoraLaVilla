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
        public async Task CrearLoteProducto_ModoPeso_DebeGuardarElStockEnKilogramos()
        {
            var capturado = new LotesProductosEntity();
            _productosRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto",
                Estado = 1,
                ModoInventario = 2,
                VentaPorPeso = true,
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

            // En un modo por peso el contador autoritativo es el kilogramo, no la cantidad de piezas.
            capturado.CantidadUnidades.Should().Be(8);
            capturado.CantidadInicial.Should().Be(4m);
            capturado.CantidadDisponible.Should().Be(4m);
            capturado.PesoDisponible.Should().Be(4m);
            capturado.PrecioTotal.Should().Be(80m);
            capturado.PrecioKilo.Should().Be(20m);
            capturado.PrecioUnitario.Should().Be(0m);
            capturado.IdUnidadMedida.Should().Be(1);
        }

        [Fact]
        public async Task CrearLoteProducto_ModoUnidad_DebeGuardarElStockEnUnidades()
        {
            var capturado = new LotesProductosEntity();
            _productosRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto",
                Estado = 1,
                ModoInventario = 1,
                VentaPorPeso = false
            });

            _lotesRepo.Setup(r => r.CreateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaVencimiento = DateTime.Today.AddDays(10),
                CantidadUnidades = 10,
                PesoTotal = 0m,
                IdUnidadMedida = 2,
                PrecioUnitario = 15m,
                PrecioKilo = 0m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.CrearLoteProductoAsync(dto);

            // Regression: el stock de un producto por unidad no puede salir del campo peso.
            capturado.CantidadInicial.Should().Be(10m);
            capturado.CantidadDisponible.Should().Be(10m);
            capturado.PesoDisponible.Should().Be(0m);
            capturado.PrecioTotal.Should().Be(150m);
            capturado.PrecioUnitario.Should().Be(15m);
            capturado.PrecioKilo.Should().Be(0m);
            capturado.IdUnidadMedida.Should().Be(2);
        }

        [Fact]
        public async Task ActualizarLoteProducto_NoDebeReemplazarFechaEntrada()
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
            capturado.FechaVencimiento.Should().Be(dto.FechaVencimiento);
            // El total del lote pasa a ser el informado; nada se consumió, así que todo queda disponible.
            capturado.CantidadInicial.Should().Be(12m);
            capturado.CantidadDisponible.Should().Be(12m);
        }

        [Fact]
        public async Task ActualizarLoteProducto_ModoUnidad_DebePreservarLoConsumido()
        {
            var lote = new LotesProductosEntity
            {
                Id = 16,
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaEntrada = new DateTime(2026, 1, 10),
                FechaVencimiento = new DateTime(2026, 2, 10),
                CantidadUnidades = 10,
                PesoTotal = 0m,
                IdUnidadMedida = 2,
                PrecioUnitario = 12m,
                PrecioKilo = 0m,
                PrecioTotal = 120m,
                IdMarca = 1,
                CantidadInicial = 10m,
                CantidadDisponible = 8m,
                PesoDisponible = 0m,
                Estado = 1
            };

            LotesProductosEntity? capturado = null;

            _lotesRepo.Setup(r => r.FindByIdAsync(16)).ReturnsAsync(lote);
            _lotesRepo.Setup(r => r.UpdateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new LotesProductosDTO
            {
                IdProducto = 1,
                IdProveedor = lote.IdProveedor,
                FechaVencimiento = new DateTime(2026, 3, 10),
                CantidadUnidades = 12,
                PesoTotal = 0m,
                IdUnidadMedida = 2,
                PrecioUnitario = 15m,
                PrecioKilo = 0m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.ActualizarLoteProducto(16, dto);

            capturado.Should().NotBeNull();
            // Se consumieron 2 unidades (10 - 8): el nuevo total deja 10 disponibles.
            capturado!.CantidadInicial.Should().Be(12m);
            capturado.CantidadDisponible.Should().Be(10m);
            capturado.PesoDisponible.Should().Be(0m);
        }

        [Fact]
        public async Task ActualizarLoteProducto_ModoPeso_DebeAjustarKilogramosYNoUnidades()
        {
            _productosRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new ProductosEntity
            {
                Id = 1,
                Nombre = "Producto",
                Estado = 1,
                ModoInventario = 2,
                VentaPorPeso = true
            });

            var lote = new LotesProductosEntity
            {
                Id = 15,
                IdProducto = 1,
                IdProveedor = Guid.NewGuid(),
                FechaEntrada = new DateTime(2026, 1, 10),
                FechaVencimiento = new DateTime(2026, 2, 10),
                CantidadUnidades = 0,
                PesoTotal = 10000m,
                IdUnidadMedida = 1,
                PrecioUnitario = 0m,
                PrecioKilo = 12m,
                PrecioTotal = 120000m,
                IdMarca = 1,
                CantidadInicial = 10000m,
                CantidadDisponible = 8000m,
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
                CantidadUnidades = 0,
                PesoTotal = 12000m,
                IdUnidadMedida = 1,
                PrecioUnitario = 0m,
                PrecioKilo = 15m,
                IdMarca = 1,
                IdUsuario = Guid.NewGuid()
            };

            await _service.ActualizarLoteProducto(15, dto);

            capturado.Should().NotBeNull();
            // 2000 kg ya consumidos sobre 10000: el nuevo total de 12000 deja 10000 kg disponibles,
            // y el contador de unidades no participa de la cuenta.
            capturado!.CantidadInicial.Should().Be(12000m);
            capturado.CantidadDisponible.Should().Be(10000m);
            capturado.PesoDisponible.Should().Be(10000m);
            capturado.PrecioTotal.Should().Be(180000m);
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
