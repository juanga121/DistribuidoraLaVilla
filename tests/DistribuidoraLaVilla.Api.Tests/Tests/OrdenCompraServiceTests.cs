using DistribuidoraLaVilla.Application.Services.Compras;
using DistribuidoraLaVilla.Domain.DTOS.Compras;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Compras;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class OrdenCompraServiceTests
    {
        private readonly Mock<IGenericRepository<OrdenCompraEntity, int>> _ordenRepo = new();
        private readonly Mock<IGenericRepository<DetalleCompraEntity, int>> _detalleRepo = new();
        private readonly Mock<IGenericRepository<ProveedoresEntity, Guid>> _proveedorRepo = new();
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productoRepo = new();
        private readonly Mock<IGenericRepository<MarcasEntity, int>> _marcaRepo = new();
        private readonly Mock<IGenericRepository<RecepcionCompraEntity, int>> _recepcionRepo = new();
        private readonly Mock<IGenericRepository<CuentasPagarEntity, int>> _cxpRepo = new();
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesRepo = new();
        private readonly Mock<IGenericRepository<MovimientosProductosEntity, int>> _movimientosRepo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly OrdenCompraService _service;

        private static readonly Guid ProveedorId = Guid.NewGuid();
        private static readonly Guid UsuarioId = Guid.NewGuid();

        public OrdenCompraServiceTests()
        {
            _service = new OrdenCompraService(
                _ordenRepo.Object,
                _detalleRepo.Object,
                _proveedorRepo.Object,
                _productoRepo.Object,
                _marcaRepo.Object,
                _recepcionRepo.Object,
                _cxpRepo.Object,
                _lotesRepo.Object,
                _movimientosRepo.Object,
                _uow.Object);

            _uow.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
            _uow.Setup(u => u.CommitAsync(default)).Returns(Task.CompletedTask);
            _uow.Setup(u => u.RollbackAsync(default)).Returns(Task.CompletedTask);
        }

        private static OrdenCompraEntity CreateOrden(int id = 10, int estado = (int)EstadoCompraEnum.Aprobada) => new()
        {
            Id = id,
            IdProveedor = ProveedorId,
            Estado = estado,
            FechaEmision = DateTime.Now.AddDays(-2),
            Subtotal = 1000m,
            Descuento = 0m,
            Impuesto = 0m,
            Total = 1000m,
            FechaCreacion = DateTime.Now.AddDays(-2)
        };

        private static ProveedoresEntity CreateProveedor() => new()
        {
            IdProveedor = ProveedorId,
            Nombre = "Proveedor Test",
            Estado = 1,
            FechaCreacion = DateTime.Now
        };

        private static RegistrarRecepcionCompraDTO CreateDto() => new()
        {
            NumeroFacturaProveedor = "F001-000123",
            FechaFactura = DateTime.Now.AddHours(-4),
            FechaRecepcion = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(30),
            FechaVencimientoLotes = DateTime.Now.AddDays(45),
            IdMarca = 7,
            Observaciones = "Recepción test",
            Detalles = new List<RegistrarRecepcionCompraDetalleDTO>
            {
                new()
                {
                    IdProducto = 1,
                    FechaVencimientoLote = DateTime.Now.AddDays(45),
                    CantidadUnidades = 5m,
                    PesoTotal = 5m,
                    IdUnidadMedida = 2,
                    PrecioUnitario = 20m,
                    PrecioKilo = 20m
                },
                new()
                {
                    IdProducto = 2,
                    FechaVencimientoLote = DateTime.Now.AddDays(45),
                    CantidadUnidades = 6m,
                    PesoTotal = 3m,
                    IdUnidadMedida = 1,
                    PrecioUnitario = 50m,
                    PrecioKilo = 50m
                }
            }
        };

        private void SetupCommon()
        {
            _proveedorRepo.Setup(r => r.FindByIdAsync(ProveedorId)).ReturnsAsync(CreateProveedor());
            _marcaRepo.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(new MarcasEntity { IdMarca = 7, Nombre = "Marca Test", Estado = 1 });
            _recepcionRepo.Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<Func<RecepcionCompraEntity, bool>>>()))
                .Returns(new List<RecepcionCompraEntity>());
            _detalleRepo.Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<Func<DetalleCompraEntity, bool>>>()))
                .Returns(new List<DetalleCompraEntity>
                {
                    new() { Id = 1, IdOrdenCompra = 10, IdProducto = 1, Cantidad = 5m, PrecioUnitario = 20m, Subtotal = 100m },
                    new() { Id = 2, IdOrdenCompra = 10, IdProducto = 2, Cantidad = 3m, PrecioUnitario = 50m, Subtotal = 150m }
                });

            _productoRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new ProductosEntity { Id = 1, Nombre = "Producto A", VentaPorPeso = false, Estado = 1 });
            _productoRepo.Setup(r => r.FindByIdAsync(2)).ReturnsAsync(new ProductosEntity { Id = 2, Nombre = "Producto B", VentaPorPeso = true, PesoPorUnidad = 0.5m, Estado = 1 });

            _ordenRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(CreateOrden());

            _recepcionRepo.Setup(r => r.CreateAsync(It.IsAny<RecepcionCompraEntity>())).Returns(Task.CompletedTask);
            _cxpRepo.Setup(r => r.CreateAsync(It.IsAny<CuentasPagarEntity>())).Returns(Task.CompletedTask);
            _lotesRepo.Setup(r => r.CreateAsync(It.IsAny<LotesProductosEntity>())).Returns(Task.CompletedTask);
            _movimientosRepo.Setup(r => r.CreateAsync(It.IsAny<MovimientosProductosEntity>())).Returns(Task.CompletedTask);
            _ordenRepo.Setup(r => r.UpdateAsync(It.IsAny<OrdenCompraEntity>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task RegistrarRecepcionCompraAsync_DeberiaCrearRecepcionCxPYLotesYMarcarOrdenRecibida()
        {
            SetupCommon();
            var dto = CreateDto();

            RecepcionCompraEntity? recepcionCapturada = null;
            CuentasPagarEntity? cxpCapturada = null;
            var lotesCapturados = new List<LotesProductosEntity>();
            var movimientosCapturados = new List<MovimientosProductosEntity>();
            OrdenCompraEntity? ordenCapturada = null;

            _recepcionRepo.Setup(r => r.CreateAsync(It.IsAny<RecepcionCompraEntity>()))
                .Callback<RecepcionCompraEntity>(e => recepcionCapturada = e)
                .Returns(Task.CompletedTask);
            _cxpRepo.Setup(r => r.CreateAsync(It.IsAny<CuentasPagarEntity>()))
                .Callback<CuentasPagarEntity>(e => cxpCapturada = e)
                .Returns(Task.CompletedTask);
            _lotesRepo.Setup(r => r.CreateAsync(It.IsAny<LotesProductosEntity>()))
                .Callback<LotesProductosEntity>(e => lotesCapturados.Add(e))
                .Returns(Task.CompletedTask);
            _movimientosRepo.Setup(r => r.CreateAsync(It.IsAny<MovimientosProductosEntity>()))
                .Callback<MovimientosProductosEntity>(e => movimientosCapturados.Add(e))
                .Returns(Task.CompletedTask);
            _ordenRepo.Setup(r => r.UpdateAsync(It.IsAny<OrdenCompraEntity>()))
                .Callback<OrdenCompraEntity>(e => ordenCapturada = e)
                .Returns(Task.CompletedTask);

            await _service.RegistrarRecepcionCompraAsync(10, dto, UsuarioId);

            recepcionCapturada.Should().NotBeNull();
            recepcionCapturada!.IdOrdenCompra.Should().Be(10);
            recepcionCapturada.NumeroFacturaProveedor.Should().Be(dto.NumeroFacturaProveedor);
            recepcionCapturada.IdMarca.Should().Be(dto.IdMarca);

            cxpCapturada.Should().NotBeNull();
            cxpCapturada!.IdOrdenCompra.Should().Be(10);
            cxpCapturada.MontoTotal.Should().Be(1000m);

            lotesCapturados.Should().HaveCount(2);
            lotesCapturados[0].IdUnidadMedida.Should().Be(2);
            lotesCapturados[1].IdUnidadMedida.Should().Be(1);
            lotesCapturados[1].PesoDisponible.Should().Be(3m);

            movimientosCapturados.Should().HaveCount(2);
            ordenCapturada.Should().NotBeNull();
            ordenCapturada!.Estado.Should().Be((int)EstadoCompraEnum.Recibida);
            ordenCapturada.FechaRecepcion.Should().BeCloseTo(dto.FechaRecepcion, TimeSpan.FromSeconds(1));

            _uow.Verify(u => u.BeginTransactionAsync(default), Times.Once);
            _uow.Verify(u => u.CommitAsync(default), Times.Once);
            _uow.Verify(u => u.RollbackAsync(default), Times.Never);
        }

        [Fact]
        public async Task RegistrarRecepcionCompraAsync_CuandoOrdenNoEstaAprobada_DeberiaFallar()
        {
            _ordenRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(CreateOrden(estado: (int)EstadoCompraEnum.Pendiente));
            var dto = CreateDto();

            var act = async () => await _service.RegistrarRecepcionCompraAsync(10, dto, UsuarioId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*aprobadas*");
        }

        [Fact]
        public async Task ActualizarEstadoOrdenAsync_CuandoQuierePasarARecibida_DeberiaFallar()
        {
            _ordenRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(CreateOrden(estado: (int)EstadoCompraEnum.Aprobada));

            var act = async () => await _service.ActualizarEstadoOrdenAsync(10, (int)EstadoCompraEnum.Recibida, UsuarioId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*flujo de recepción*");
        }

        [Fact]
        public async Task ActualizarEstadoOrdenAsync_CuandoCancelaOrdenAprobada_DeberiaPermitirlo()
        {
            var orden = CreateOrden(estado: (int)EstadoCompraEnum.Aprobada);
            _ordenRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(orden);

            await _service.ActualizarEstadoOrdenAsync(10, (int)EstadoCompraEnum.Cancelada, UsuarioId);

            _ordenRepo.Verify(r => r.UpdateAsync(It.Is<OrdenCompraEntity>(o => o.Estado == (int)EstadoCompraEnum.Cancelada)), Times.Once);
        }
    }
}
