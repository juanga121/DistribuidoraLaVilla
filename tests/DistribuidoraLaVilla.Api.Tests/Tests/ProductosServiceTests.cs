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
        private readonly ProductosService _service;

        public ProductosServiceTests()
        {
            _service = new ProductosService(_productosRepo.Object, _auditoriaService.Object);
            _productosRepo.Setup(r => r.CreateAsync(It.IsAny<ProductosEntity>())).Returns(Task.CompletedTask);
            _productosRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductosEntity>())).Returns(Task.CompletedTask);
            _productosRepo.Setup(r => r.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProductosEntity { Id = 1, Nombre = "Producto", Estado = 1 });
        }

        [Fact]
        public async Task CrearProducto_CuandoEsVentaPorPesoYPreciosIguales_DeberiaFallar()
        {
            var dto = new ProductoDTO
            {
                Nombre = "Queso",
                Descripcion = "Queso test",
                IdCategoria = 1,
                PrecioUnitario = 12000m,
                VentaPorPeso = true,
                PrecioPorKilo = 12000m,
                PesoPorUnidad = 1.2m
            };

            var act = async () => await _service.CrearProductoAsync(dto, Guid.NewGuid());

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*deben ser distintos*");
        }

        [Fact]
        public async Task CrearProducto_CuandoEsVentaPorPesoYFaltaPesoPorUnidad_DeberiaFallar()
        {
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

            var act = async () => await _service.CrearProductoAsync(dto, Guid.NewGuid());

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*peso por unidad*obligatorio*");
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
