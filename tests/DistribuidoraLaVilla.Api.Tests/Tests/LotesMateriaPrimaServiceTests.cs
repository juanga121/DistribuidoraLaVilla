using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class LotesMateriaPrimaServiceTests
    {
        private readonly Mock<IGenericRepository<LotesMateriaPrimaEntity, int>> _lotesRepoMock;
        private readonly Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>> _movimientosRepoMock;
        private readonly Mock<IGenericRepository<MarcasEntity, int>> _marcasRepoMock;
        private readonly Mock<IGenericRepository<MateriaPrimaEntity, int>> _materiaPrimaRepoMock;
        private readonly Mock<IGenericRepository<ProveedoresEntity, Guid>> _proveedoresRepoMock;
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepoMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly LotesMateriaPrimaService _service;

        public LotesMateriaPrimaServiceTests()
        {
            _lotesRepoMock = new Mock<IGenericRepository<LotesMateriaPrimaEntity, int>>();
            _movimientosRepoMock = new Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>>();
            _marcasRepoMock = new Mock<IGenericRepository<MarcasEntity, int>>();
            _materiaPrimaRepoMock = new Mock<IGenericRepository<MateriaPrimaEntity, int>>();
            _proveedoresRepoMock = new Mock<IGenericRepository<ProveedoresEntity, Guid>>();
            _unidadMedidaRepoMock = new Mock<IGenericRepository<UnidadMedidaEntity, int>>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _service = new LotesMateriaPrimaService(
                _lotesRepoMock.Object,
                _movimientosRepoMock.Object,
                _marcasRepoMock.Object,
                _materiaPrimaRepoMock.Object,
                _proveedoresRepoMock.Object,
                _unidadMedidaRepoMock.Object,
                _auditoriaServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        private static LotesMateriaPrimaDTO CrearDtoValido()
        {
            return new LotesMateriaPrimaDTO
            {
                IdMarca = 1,
                IdMateria = 1,
                IdProveedor = Guid.NewGuid(),
                FechaVencimiento = DateTime.Now.AddDays(30),
                Cantidad = 100,
                IdUnidadMedida = 1,
                CostoUnitario = 50.75m,
                IdUsuario = Guid.NewGuid()
            };
        }

        #region CrearLoteMateriaPrimaAsync

        [Fact]
        public async Task CrearLoteMateriaPrimaAsync_DeberiaCrearLoteYMovimientoEnMismaTransaccion()
        {
            // Arrange
            var dto = CrearDtoValido();
            var loteCreado = new LotesMateriaPrimaEntity { Id = 42 }; // ID asignado después de CreateAsync

            _lotesRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Callback<LotesMateriaPrimaEntity>(e =>
                {
                    // Simular que la BD asigna el ID
                    e.Id = loteCreado.Id;
                    e.CantidadInicial = dto.Cantidad;
                    e.CantidadDisponible = dto.Cantidad;
                    e.CostoTotal = dto.Cantidad * dto.CostoUnitario;
                });

            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _auditoriaServiceMock
                .Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.CrearLoteMateriaPrimaAsync(dto);

            // Assert
            // Verificar que se creó el lote
            _lotesRepoMock.Verify(r => r.CreateAsync(It.Is<LotesMateriaPrimaEntity>(
                e => e.IdMarca == dto.IdMarca
                  && e.IdMateria == dto.IdMateria
                  && e.Cantidad == dto.Cantidad
                  && e.CantidadDisponible == dto.Cantidad
                  && e.CantidadInicial == dto.Cantidad
                  && e.Estado == 1
            )), Times.Once);

            // Verificar que se creó el movimiento de entrada (IdTipoMovimiento = 1)
            _movimientosRepoMock.Verify(r => r.CreateAsync(It.Is<MovimientosMateriaPrimaEntity>(
                m => m.IdLoteMateria == loteCreado.Id
                  && m.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Entrada
                  && m.Cantidad == dto.Cantidad
                  && m.IdUnidadMedida == dto.IdUnidadMedida
                  && m.IdUsuario == dto.IdUsuario
            )), Times.Once);

            // Verificar que se inició la transacción y se hizo commit (no rollback)
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar que DisposeAsync se llamó en el finally
            _unitOfWorkMock.Verify(u => u.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task CrearLoteMateriaPrimaAsync_DeberiaHacerRollbackSiFallaMovimiento()
        {
            // Arrange
            var dto = CrearDtoValido();
            var excepcionEsperada = new InvalidOperationException("Error al crear movimiento");

            _lotesRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Callback<LotesMateriaPrimaEntity>(e => e.Id = 42);

            _auditoriaServiceMock
                .Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            // Simular fallo al crear el movimiento
            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()))
                .ThrowsAsync(excepcionEsperada);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.CrearLoteMateriaPrimaAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            // Verificar que se hizo rollback (no commit)
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar que DisposeAsync se llamó en el finally
            _unitOfWorkMock.Verify(u => u.DisposeAsync(), Times.Once);
        }

        #endregion

        #region ActualizarLoteMateriaPrima

        [Fact]
        public async Task ActualizarLoteMateriaPrima_CuandoCambiaCantidad_DeberiaCrearMovimientoAjuste()
        {
            // Arrange
            var loteId = 10;
            var dto = CrearDtoValido();
            dto.Cantidad = 150; // cantidad nueva

            var loteExistente = new LotesMateriaPrimaEntity
            {
                Id = loteId,
                IdMarca = 1,
                IdMateria = 1,
                IdProveedor = dto.IdProveedor,
                FechaEntrada = DateTime.Now.AddDays(-10),
                FechaVencimiento = DateTime.Now.AddDays(20),
                Cantidad = 100,           // cantidad anterior = 100
                CantidadDisponible = 80,
                CantidadInicial = 100,
                IdUnidadMedida = 1,
                CostoUnitario = 50.75m,
                CostoTotal = 5075m,
                Estado = 1
            };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(loteId))
                .ReturnsAsync(loteExistente);

            _lotesRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _auditoriaServiceMock
                .Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ActualizarLoteMateriaPrima(loteId, dto);

            // Assert
            // Verificar que se creó un movimiento de tipo Ajuste (IdTipoMovimiento = 3)
            // La diferencia es 150 - 100 = 50
            _movimientosRepoMock.Verify(r => r.CreateAsync(It.Is<MovimientosMateriaPrimaEntity>(
                m => m.IdLoteMateria == loteId
                  && m.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Ajuste
                  && m.Cantidad == 50 // diferencia positiva
                  && m.IdUnidadMedida == dto.IdUnidadMedida
                  && m.IdUsuario == dto.IdUsuario
            )), Times.Once);

            // Verificar transacción exitosa
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar que CantidadDisponible se actualizó correctamente (80 + 50 = 130)
            _lotesRepoMock.Verify(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(
                l => l.CantidadDisponible == 130
                  && l.Cantidad == 150
                  && l.CostoTotal == 150 * 50.75m
            )), Times.Once);
        }

        [Fact]
        public async Task ActualizarLoteMateriaPrima_CuandoNoCambiaCantidad_NoDeberiaCrearMovimiento()
        {
            // Arrange
            var loteId = 10;
            var dto = CrearDtoValido();
            dto.Cantidad = 100; // mismo valor que la cantidad anterior

            var loteExistente = new LotesMateriaPrimaEntity
            {
                Id = loteId,
                IdMarca = 1,
                IdMateria = 1,
                IdProveedor = dto.IdProveedor,
                FechaEntrada = DateTime.Now.AddDays(-10),
                FechaVencimiento = DateTime.Now.AddDays(20),
                Cantidad = 100,           // cantidad anterior = 100 (igual a la nueva)
                CantidadDisponible = 80,
                CantidadInicial = 100,
                IdUnidadMedida = 1,
                CostoUnitario = 50.75m,
                CostoTotal = 5075m,
                Estado = 1
            };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(loteId))
                .ReturnsAsync(loteExistente);

            _lotesRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _auditoriaServiceMock
                .Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ActualizarLoteMateriaPrima(loteId, dto);

            // Assert
            // NO debe crear movimiento porque la cantidad no cambió (diferencia = 0)
            _movimientosRepoMock.Verify(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()), Times.Never);

            // La transacción debe completarse exitosamente
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
