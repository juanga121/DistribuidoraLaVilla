using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class CuentasPagarServiceTests
    {
        private readonly Mock<IGenericRepository<CuentasPagarEntity, int>> _cxpRepoMock;
        private readonly Mock<IGenericRepository<ProveedoresEntity, Guid>> _proveedoresRepoMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly MovimientosFinancierosService _movimientosService;
        private readonly CuentasPagarService _service;

        private static readonly Guid ProveedorId = Guid.NewGuid();
        private static readonly Guid UsuarioId = Guid.NewGuid();

        public CuentasPagarServiceTests()
        {
            _cxpRepoMock = new Mock<IGenericRepository<CuentasPagarEntity, int>>();
            _proveedoresRepoMock = new Mock<IGenericRepository<ProveedoresEntity, Guid>>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _movimientosService = new MovimientosFinancierosService(
                new Mock<IGenericRepository<MovimientosFinancierosEntity, int>>().Object);

            _service = new CuentasPagarService(
                _cxpRepoMock.Object,
                _proveedoresRepoMock.Object,
                _auditoriaServiceMock.Object,
                _movimientosService);
        }

        // ── Helpers ──

        private static ProveedoresEntity CreateProveedor(Guid? id = null) => new()
        {
            IdProveedor = id ?? ProveedorId,
            Nombre = "Proveedor Test",
            Estado = 1,
            FechaCreacion = DateTime.Now
        };

        private static CrearCxPDTO CreateDto(decimal monto = 1000m) => new()
        {
            IdProveedor = ProveedorId,
            IdOrdenCompra = null,
            MontoTotal = monto,
            FechaVencimiento = DateTime.Now.AddDays(30),
            IdUsuario = UsuarioId
        };

        private void SetupProveedor(Guid? id = null)
        {
            _proveedoresRepoMock
                .Setup(r => r.FindByIdAsync(id ?? ProveedorId))
                .ReturnsAsync(CreateProveedor(id));
        }

        // ── CrearAsync ──

        [Fact]
        public async Task CrearAsync_DeberiaCrearCxPConLosCamposCorrectos()
        {
            // Arrange
            var dto = CreateDto(2500m);
            SetupProveedor();

            CuentasPagarEntity? captured = null;
            _cxpRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<CuentasPagarEntity>()))
                .Callback<CuentasPagarEntity>(e => captured = e)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CrearAsync(dto, UsuarioId);

            // Assert
            result.Should().NotBeNull();
            result.MontoTotal.Should().Be(2500m);
            result.SaldoPendiente.Should().Be(2500m);
            result.Estado.Should().Be(1);
            result.EstadoDescripcion.Should().Be("Pendiente");
            result.IdProveedor.Should().Be(ProveedorId);

            captured.Should().NotBeNull();
            captured!.SaldoPendiente.Should().Be(2500m);
            captured.Estado.Should().Be(1);
            captured.IdUsuario.Should().Be(UsuarioId);

            _auditoriaServiceMock.Verify(a =>
                a.RegistrarAsync("CuentasPagar", It.IsAny<string>(), "Crear", It.IsAny<string>(), UsuarioId),
                Times.Once);
        }

        [Fact]
        public async Task CrearAsync_CuandoProveedorNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            var dto = CreateDto();
            _proveedoresRepoMock
                .Setup(r => r.FindByIdAsync(ProveedorId))
                .ReturnsAsync((ProveedoresEntity?)null);

            // Act
            var act = async () => await _service.CrearAsync(dto, UsuarioId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*proveedor*no existe*");
        }

        // ── ObtenerTodosAsync ──

        [Fact]
        public async Task ObtenerTodosAsync_ConEstado_DeberiaFiltrarPorEstado()
        {
            // Arrange
            var entities = new List<CuentasPagarEntity>
            {
                new() { IdCuentaPagar = 1, IdProveedor = ProveedorId, Estado = 1, MontoTotal = 500, SaldoPendiente = 500 },
                new() { IdCuentaPagar = 2, IdProveedor = ProveedorId, Estado = 2, MontoTotal = 300, SaldoPendiente = 0 },
                new() { IdCuentaPagar = 3, IdProveedor = ProveedorId, Estado = 1, MontoTotal = 700, SaldoPendiente = 700 }
            };

            _cxpRepoMock
                .Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<Func<CuentasPagarEntity, bool>>>()))
                .Returns(entities.Where(e => e.Estado == 1).ToList());

            SetupProveedor();

            // Act
            var result = await _service.ObtenerTodosAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.Estado == 1);
        }

        [Fact]
        public async Task ObtenerTodosAsync_SinEstado_DeberiaDevolverTodos()
        {
            // Arrange
            var entities = new List<CuentasPagarEntity>
            {
                new() { IdCuentaPagar = 1, IdProveedor = ProveedorId, Estado = 1, MontoTotal = 500, SaldoPendiente = 500 },
                new() { IdCuentaPagar = 2, IdProveedor = ProveedorId, Estado = 2, MontoTotal = 300, SaldoPendiente = 0 }
            };

            _cxpRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            SetupProveedor();

            // Act
            var result = await _service.ObtenerTodosAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        // ── ObtenerPorIdAsync ──

        [Fact]
        public async Task ObtenerPorIdAsync_DeberiaDevolverCxPConProveedor()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 1000,
                SaldoPendiente = 1000
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            SetupProveedor();

            // Act
            var result = await _service.ObtenerPorIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.IdCuentaPagar.Should().Be(1);
            result.ProveedorNombre.Should().Be("Proveedor Test");
            result.MontoTotal.Should().Be(1000);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_CuandoNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(999))
                .ReturnsAsync((CuentasPagarEntity?)null);

            // Act
            var act = async () => await _service.ObtenerPorIdAsync(999);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no existe*");
        }

        // ── ActualizarAsync ──

        [Fact]
        public async Task ActualizarAsync_DeberiaActualizarLosCamposCorrectamente()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 1000,
                SaldoPendiente = 1000,
                FechaCreacion = DateTime.Now.AddDays(-10)
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            SetupProveedor();

            var dto = new CrearCxPDTO
            {
                IdProveedor = ProveedorId,
                MontoTotal = 2000,
                FechaVencimiento = DateTime.Now.AddDays(60)
            };

            // Act
            var result = await _service.ActualizarAsync(1, dto, UsuarioId);

            // Assert
            result.MontoTotal.Should().Be(2000);

            _cxpRepoMock.Verify(r => r.UpdateAsync(It.Is<CuentasPagarEntity>(
                e => e.IdCuentaPagar == 1 && e.MontoTotal == 2000
            )), Times.Once);

            _auditoriaServiceMock.Verify(a =>
                a.RegistrarAsync("CuentasPagar", "1", "Actualizar", It.IsAny<string>(), UsuarioId),
                Times.Once);
        }

        // ── EliminarAsync ──

        [Fact]
        public async Task EliminarAsync_DeberiaHacerSoftDelete()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 500,
                SaldoPendiente = 500,
                IdUsuario = UsuarioId
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            // Act
            await _service.EliminarAsync(1);

            // Assert
            entity.Estado.Should().Be(3);
            _cxpRepoMock.Verify(r => r.UpdateAsync(It.Is<CuentasPagarEntity>(
                e => e.Estado == 3
            )), Times.Once);
        }

        [Fact]
        public async Task EliminarAsync_CuandoNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(999))
                .ReturnsAsync((CuentasPagarEntity?)null);

            // Act
            var act = async () => await _service.EliminarAsync(999);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        // ── RegistrarPagoAsync ──

        [Fact]
        public async Task RegistrarPagoAsync_DeberiaReducirSaldoCorrectamente()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 1000,
                SaldoPendiente = 1000,
                IdUsuario = UsuarioId
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            SetupProveedor();

            var dto = new RegistrarPagoCxPDTO { Monto = 400, IdUsuario = UsuarioId };

            // Act
            var result = await _service.RegistrarPagoAsync(1, dto, UsuarioId);

            // Assert
            result.SaldoPendiente.Should().Be(600);
            result.Estado.Should().Be(1); // Still pending

            _cxpRepoMock.Verify(r => r.UpdateAsync(It.Is<CuentasPagarEntity>(
                e => e.SaldoPendiente == 600 && e.Estado == 1
            )), Times.Once);
        }

        [Fact]
        public async Task RegistrarPagoAsync_CuandoSaldoLlegaACero_DeberiaMarcarComoPagada()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 500,
                SaldoPendiente = 500,
                IdUsuario = UsuarioId
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            SetupProveedor();

            var dto = new RegistrarPagoCxPDTO { Monto = 500, IdUsuario = UsuarioId };

            // Act
            var result = await _service.RegistrarPagoAsync(1, dto, UsuarioId);

            // Assert
            result.SaldoPendiente.Should().Be(0);
            result.Estado.Should().Be(2);
            result.EstadoDescripcion.Should().Be("Pagada");
        }

        [Fact]
        public async Task RegistrarPagoAsync_CuandoMontoExcedeSaldo_DeberiaLanzarExcepcion()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 500,
                SaldoPendiente = 200,
                IdUsuario = UsuarioId
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            var dto = new RegistrarPagoCxPDTO { Monto = 300 };

            // Act
            var act = async () => await _service.RegistrarPagoAsync(1, dto, UsuarioId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*excede*saldo*");
        }

        [Fact]
        public async Task RegistrarPagoAsync_CuandoMontoEsCero_DeberiaLanzarExcepcion()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 1,
                MontoTotal = 500,
                SaldoPendiente = 500,
                IdUsuario = UsuarioId
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            var dto = new RegistrarPagoCxPDTO { Monto = 0 };

            // Act
            var act = async () => await _service.RegistrarPagoAsync(1, dto, UsuarioId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*mayor a cero*");
        }

        [Fact]
        public async Task RegistrarPagoAsync_CuandoCxPNoEstaPendiente_DeberiaLanzarExcepcion()
        {
            // Arrange
            var entity = new CuentasPagarEntity
            {
                IdCuentaPagar = 1,
                IdProveedor = ProveedorId,
                Estado = 2, // Already paid
                MontoTotal = 500,
                SaldoPendiente = 0
            };

            _cxpRepoMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(entity);

            var dto = new RegistrarPagoCxPDTO { Monto = 100 };

            // Act
            var act = async () => await _service.RegistrarPagoAsync(1, dto, UsuarioId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no está pendiente*");
        }
    }
}
