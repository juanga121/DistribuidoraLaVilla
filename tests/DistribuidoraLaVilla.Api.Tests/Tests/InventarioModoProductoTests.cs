using System.Linq.Expressions;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Inventario;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    /// <summary>
    /// Covers the product FIFO rules around inventory modes: weight modes keep their stock in
    /// kilograms and can only be sold by weight, even when the lot carries zero units.
    /// </summary>
    public class InventarioModoProductoTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<LotesMateriaPrimaEntity, int>> _lotesMPRepoMock = new();
        private readonly Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>> _movimientosMPRepoMock = new();
        private readonly Mock<IGenericRepository<MateriaPrimaEntity, int>> _materiaPrimaRepoMock = new();
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepoMock = new();
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesProductosRepoMock = new();
        private readonly Mock<IGenericRepository<MovimientoEntity, int>> _movimientosRepoMock = new();
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productosRepoMock = new();
        private readonly InventarioService _service;

        public InventarioModoProductoTests()
        {
            _service = new InventarioService(
                _unitOfWorkMock.Object,
                _lotesMPRepoMock.Object,
                _movimientosMPRepoMock.Object,
                _materiaPrimaRepoMock.Object,
                _unidadMedidaRepoMock.Object,
                _lotesProductosRepoMock.Object,
                _movimientosRepoMock.Object,
                _productosRepoMock.Object);

            _unitOfWorkMock.Setup(u => u.HasActiveTransaction).Returns(false);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.DisposeAsync()).Returns(ValueTask.CompletedTask);

            _lotesProductosRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesProductosEntity>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoPeso_DeberiaConsumirKilogramosAunqueNoHayaUnidades()
        {
            const int productoId = 5;
            const int idUnidadMedidaKilo = 1;
            var idUsuario = Guid.NewGuid();

            var producto = new ProductosEntity
            {
                Id = productoId,
                Nombre = "Queso criollo",
                Estado = 1,
                ModoInventario = (int)ModoInventarioEnum.Peso,
                VentaPorPeso = true,
                PrecioPorKilo = 19000m
            };

            var lote = new LotesProductosEntity
            {
                Id = 11,
                IdProducto = productoId,
                Estado = 1,
                CantidadUnidades = 0,
                PesoTotal = 391.2m,
                CantidadInicial = 391.2m,
                CantidadDisponible = 391.2m,
                PesoDisponible = 391.2m,
                PrecioKilo = 15000m,
                FechaVencimiento = DateTime.Today.AddDays(30)
            };

            MovimientoEntity? movimientoCapturado = null;

            _productosRepoMock.Setup(r => r.FindByIdAsync(productoId)).ReturnsAsync(producto);
            _lotesProductosRepoMock
                .Setup(r => r.GetByFilter(It.IsAny<Expression<Func<LotesProductosEntity, bool>>>()))
                .Returns(new List<LotesProductosEntity> { lote });
            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientoEntity>()))
                .Callback<MovimientoEntity>(m => movimientoCapturado = m)
                .Returns(Task.CompletedTask);

            var consumos = await _service.ConsumirLotesProductoAsync(
                productoId, 2.5m, idUnidadMedidaKilo, idUsuario, Guid.Empty, "Venta", esVentaPorPeso: true);

            consumos.Should().ContainSingle();
            consumos[0].CantidadConsumida.Should().Be(2.5m);
            lote.PesoDisponible.Should().Be(388.7m);
            lote.CantidadDisponible.Should().Be(388.7m);

            movimientoCapturado.Should().NotBeNull();
            movimientoCapturado!.Cantidad.Should().Be(2.5m);
            movimientoCapturado.TotalMovimiento.Should().Be(2.5m * 15000m);
            movimientoCapturado.IdUnidadMedida.Should().Be(idUnidadMedidaKilo);
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoUnidad_DeberiaRechazarUnaVentaInformadaPorPeso()
        {
            const int productoId = 7;
            var producto = new ProductosEntity
            {
                Id = productoId,
                Nombre = "Pan tajado",
                Estado = 1,
                ModoInventario = (int)ModoInventarioEnum.Unidad,
                VentaPorPeso = false,
                // Un producto por unidad puede tener peso por unidad informado y aun así no venderse por kg.
                PesoPorUnidad = 0.5m
            };

            _productosRepoMock.Setup(r => r.FindByIdAsync(productoId)).ReturnsAsync(producto);

            var act = async () => await _service.ConsumirLotesProductoAsync(
                productoId, 1.5m, 1, Guid.NewGuid(), Guid.Empty, "Venta", esVentaPorPeso: true);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*se administra por unidad*");

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoPeso_DeberiaRechazarUnaVentaInformadaPorUnidad()
        {
            const int productoId = 6;
            var producto = new ProductosEntity
            {
                Id = productoId,
                Nombre = "Cuajada",
                Estado = 1,
                ModoInventario = (int)ModoInventarioEnum.Peso,
                VentaPorPeso = true,
                PrecioPorKilo = 14000m
            };

            _productosRepoMock.Setup(r => r.FindByIdAsync(productoId)).ReturnsAsync(producto);

            var act = async () => await _service.ConsumirLotesProductoAsync(
                productoId, 1m, 2, Guid.NewGuid(), Guid.Empty, "Venta", esVentaPorPeso: false);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*se administra por peso*");

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
