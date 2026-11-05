using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class ProductosServiceTests
    {
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productosRepo = new();
        private readonly Mock<IAuditoriaService> _auditoriaService = new();
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesProductosRepo = new();
        private readonly ProductosService _service;

        public ProductosServiceTests()
        {
            _service = new ProductosService(_productosRepo.Object, _auditoriaService.Object, _lotesProductosRepo.Object);
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>())).Returns(Task.CompletedTask);
            _productosRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductosEntity>())).Returns(Task.CompletedTask);
            _productosRepo.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProductosEntity { Id = 1, Nombre = "Producto", Estado = 1 });
            _lotesProductosRepo.Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<System.Func<LotesProductosEntity, bool>>>()))
                .Returns(new List<LotesProductosEntity>());
        }

        [Fact]
        public async Task CrearProducto_ModoMixtoConPreciosIguales_DeberiaFallar()
        {
            var dto = new ProductoDTO
            {
                Nombre = "Queso",
                Descripcion = "Queso test",
                IdCategoria = 1,
                PrecioUnitario = 12000m,
                ModoInventario = 4,
                PrecioPorKilo = 12000m,
                PesoPorUnidad = 1.2m
            };

            var act = async () => await _service.CrearProductoAsync(dto, Guid.NewGuid());

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*deben ser distintos*");
        }

        [Fact]
        public async Task CrearProducto_CuandoEsVentaPorPesoSinPesoPorUnidad_DeberiaPermitirlo()
        {
            ProductosEntity? capturado = null;
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>()))
                .Callback<ProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new ProductoDTO
            {
                Nombre = "Queso",
                Descripcion = "Queso test",
                IdCategoria = 1,
                PrecioUnitario = 12000m,
                VentaPorPeso = true,
                PrecioPorKilo = 10000m,
                PesoPorUnidad = null
            };

            await _service.CrearProductoAsync(dto, Guid.NewGuid());

            capturado.Should().NotBeNull();
            capturado!.VentaPorPeso.Should().BeTrue();
            capturado.PesoPorUnidad.Should().BeNull();
            capturado.ModoInventario.Should().Be(2);
        }

        [Fact]
        public async Task CrearProducto_ModoUnidad_LimpiaValoresDePeso()
        {
            ProductosEntity? capturado = null;
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>()))
                .Callback<ProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new ProductoDTO
            {
                Nombre = "Harina",
                Descripcion = "Harina test",
                IdCategoria = 1,
                PrecioUnitario = 5000m,
                ModoInventario = 1,
                PrecioPorKilo = 7000m,
                PesoPorUnidad = 0.25m
            };

            await _service.CrearProductoAsync(dto, Guid.NewGuid());

            capturado.Should().NotBeNull();
            capturado!.ModoInventario.Should().Be(1);
            capturado.VentaPorPeso.Should().BeFalse();
            capturado.PrecioPorKilo.Should().BeNull();
            capturado.PesoPorUnidad.Should().BeNull();
        }

        [Fact]
        public async Task CrearProducto_ModoPeso_SinPesoPorUnidad_DeberiaPermitirlo()
        {
            ProductosEntity? capturado = null;
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>()))
                .Callback<ProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new ProductoDTO
            {
                Nombre = "Queso al peso",
                Descripcion = "Producto al peso",
                IdCategoria = 1,
                PrecioUnitario = 0m,
                ModoInventario = 2,
                PrecioPorKilo = 8500m,
                PesoPorUnidad = null
            };

            await _service.CrearProductoAsync(dto, Guid.NewGuid());

            capturado.Should().NotBeNull();
            capturado!.ModoInventario.Should().Be(2);
            capturado.VentaPorPeso.Should().BeTrue();
            capturado.PrecioPorKilo.Should().Be(8500m);
            capturado.PesoPorUnidad.Should().BeNull();
        }

        [Fact]
        public async Task CrearProducto_CuandoNoEsVentaPorPeso_DeberiaLimpiarValoresDerivados()
        {
            ProductosEntity? capturado = null;
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>()))
                .Callback<ProductosEntity>(e => capturado = e)
                .Returns(Task.CompletedTask);

            var dto = new ProductoDTO
            {
                Nombre = "Harina",
                Descripcion = "Harina test",
                IdCategoria = 1,
                PrecioUnitario = 5000m,
                VentaPorPeso = false,
                PrecioPorKilo = 7000m,
                PesoPorUnidad = 0.25m
            };

            await _service.CrearProductoAsync(dto, Guid.NewGuid());

            capturado.Should().NotBeNull();
            capturado!.PrecioPorKilo.Should().BeNull();
            capturado.PesoPorUnidad.Should().BeNull();
        }
    }
}
