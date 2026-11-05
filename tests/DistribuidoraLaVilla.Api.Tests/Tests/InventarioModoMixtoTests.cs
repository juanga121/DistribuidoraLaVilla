using System.Linq.Expressions;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Inventario;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    /// <summary>
    /// Covers the mixed mode: a product whose stock is kept in kilograms but that can be sold both
    /// by kilogram and by unit, converting the unit sale with its fixed weight per unit.
    /// </summary>
    public class InventarioModoMixtoTests
    {
        private const int ProductoId = 9;

        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<LotesMateriaPrimaEntity, int>> _lotesMPRepoMock = new();
        private readonly Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>> _movimientosMPRepoMock = new();
        private readonly Mock<IGenericRepository<MateriaPrimaEntity, int>> _materiaPrimaRepoMock = new();
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepoMock = new();
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesProductosRepoMock = new();
        private readonly Mock<IGenericRepository<MovimientoEntity, int>> _movimientosRepoMock = new();
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productosRepoMock = new();
        private readonly InventarioService _service;

        public InventarioModoMixtoTests()
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
            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientoEntity>()))
                .Returns(Task.CompletedTask);
        }

        private ProductosEntity ProductoMixto(decimal? pesoPorUnidad = 0.5m) => new()
        {
            Id = ProductoId,
            Nombre = "Redondo campiña",
            Estado = 1,
            ModoInventario = (int)ModoInventarioEnum.UnidadYPeso,
            VentaPorPeso = true,
            PesoPorUnidad = pesoPorUnidad,
            PrecioUnitario = 9000m,
            PrecioPorKilo = 18000m
        };

        private LotesProductosEntity LoteConStock() => new()
        {
            Id = 21,
            IdProducto = ProductoId,
            Estado = 1,
            CantidadUnidades = 0,
            PesoTotal = 100m,
            CantidadInicial = 100m,
            CantidadDisponible = 100m,
            PesoDisponible = 100m,
            PrecioKilo = 10000m,
            PrecioUnitario = 5000m,
            FechaVencimiento = DateTime.Today.AddDays(30)
        };

        private void Preparar(ProductosEntity producto, LotesProductosEntity lote)
        {
            _productosRepoMock.Setup(r => r.FindByIdAsync(ProductoId)).ReturnsAsync(producto);
            _lotesProductosRepoMock
                .Setup(r => r.GetByFilter(It.IsAny<Expression<Func<LotesProductosEntity, bool>>>()))
                .Returns(new List<LotesProductosEntity> { lote });
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoMixtoVentaPorUnidad_DeberiaDescontarKilogramosEInformarUnidades()
        {
            var producto = ProductoMixto(pesoPorUnidad: 0.5m);
            var lote = LoteConStock();
            Preparar(producto, lote);

            // 4 unidades pesadas en el mostrador: 2,1 kg reales (el nominal de 0,5 no se usa)
            var consumos = await _service.ConsumirLotesProductoAsync(
                ProductoId, 4m, 2, Guid.NewGuid(), Guid.Empty, "Venta", esVentaPorPeso: false,
                pesoRealLinea: 2.1m);

            consumos.Should().ContainSingle();
            // La factura sigue informando unidades.
            consumos[0].CantidadConsumida.Should().Be(4m);

            // El stock baja el peso real pesado, no el nominal.
            lote.PesoDisponible.Should().Be(97.9m);
            lote.CantidadDisponible.Should().Be(97.9m);
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoMixtoVentaPorPeso_DeberiaDescontarKilogramos()
        {
            var producto = ProductoMixto();
            var lote = LoteConStock();
            Preparar(producto, lote);

            var consumos = await _service.ConsumirLotesProductoAsync(
                ProductoId, 3m, 1, Guid.NewGuid(), Guid.Empty, "Venta", esVentaPorPeso: true);

            consumos.Should().ContainSingle();
            consumos[0].CantidadConsumida.Should().Be(3m);

            lote.PesoDisponible.Should().Be(97m);
            lote.CantidadDisponible.Should().Be(97m);
        }

        [Fact]
        public async Task ConsumirLotesProductoAsync_ModoMixtoSinPesoReal_DeberiaFallar()
        {
            var producto = ProductoMixto(pesoPorUnidad: null);
            var lote = LoteConStock();
            Preparar(producto, lote);

            // Vender por unidad sin informar el peso pesado ya no está permitido.
            var act = async () => await _service.ConsumirLotesProductoAsync(
                ProductoId, 2m, 2, Guid.NewGuid(), Guid.Empty, "Venta", esVentaPorPeso: false);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*hay que informar el peso*");
        }
    }
}
