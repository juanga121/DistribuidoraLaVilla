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
    public class InventarioServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<LotesMateriaPrimaEntity, int>> _lotesMPRepoMock;
        private readonly Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>> _movimientosMPRepoMock;
        private readonly Mock<IGenericRepository<MateriaPrimaEntity, int>> _materiaPrimaRepoMock;
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepoMock;
        private readonly Mock<IGenericRepository<LotesProductosEntity, int>> _lotesProductosRepoMock;
        private readonly Mock<IGenericRepository<MovimientoEntity, int>> _movimientosRepoMock;
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productosRepoMock;
        private readonly InventarioService _service;

        public InventarioServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _lotesMPRepoMock = new Mock<IGenericRepository<LotesMateriaPrimaEntity, int>>();
            _movimientosMPRepoMock = new Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>>();
            _materiaPrimaRepoMock = new Mock<IGenericRepository<MateriaPrimaEntity, int>>();
            _unidadMedidaRepoMock = new Mock<IGenericRepository<UnidadMedidaEntity, int>>();
            _lotesProductosRepoMock = new Mock<IGenericRepository<LotesProductosEntity, int>>();
            _movimientosRepoMock = new Mock<IGenericRepository<MovimientoEntity, int>>();
            _productosRepoMock = new Mock<IGenericRepository<ProductosEntity, int>>();

            _service = new InventarioService(
                _unitOfWorkMock.Object,
                _lotesMPRepoMock.Object,
                _movimientosMPRepoMock.Object,
                _materiaPrimaRepoMock.Object,
                _unidadMedidaRepoMock.Object,
                _lotesProductosRepoMock.Object,
                _movimientosRepoMock.Object,
                _productosRepoMock.Object
            );
        }

        [Fact]
        public async Task ConsumirLotesMateriaPrimaAsync_DeberiaDescontarStockYCrearMovimientosEnUnaTransaccion()
        {
            // Arrange
            var materiaPrimaId = 1;
            var cantidadRequerida = 30m;
            var idUnidadMedida = 1;
            var idUsuario = Guid.NewGuid();
            var observacion = "Consumo para test";

            // Crear materia prima
            var materiaPrima = new MateriaPrimaEntity
            {
                Id = materiaPrimaId,
                Nombre = "Harina",
            };

            // Crear unidad de medida
            var unidadMedida = new UnidadMedidaEntity
            {
                Id = 1,
                Nombre = "Kilogramo",
                Abreviatura = "kg"
            };

            // Crear lotes disponibles (FIFO: ordenados por FechaVencimiento)
            var lotes = new List<LotesMateriaPrimaEntity>
            {
                new()
                {
                    Id = 1,
                    IdMateria = materiaPrimaId,
                    CantidadDisponible = 20m,
                    Cantidad = 20m,
                    FechaVencimiento = DateTime.Now.AddDays(10),
                    Estado = 1,
                    IdUnidadMedida = 1
                },
                new()
                {
                    Id = 2,
                    IdMateria = materiaPrimaId,
                    CantidadDisponible = 20m,
                    Cantidad = 20m,
                    FechaVencimiento = DateTime.Now.AddDays(20),
                    Estado = 1,
                    IdUnidadMedida = 1
                }
            };

            _unitOfWorkMock
                .Setup(u => u.HasActiveTransaction)
                .Returns(false);

            _materiaPrimaRepoMock
                .Setup(r => r.FindByIdAsync(materiaPrimaId))
                .ReturnsAsync(materiaPrima);

            _unidadMedidaRepoMock
                .Setup(r => r.FindByIdAsync(idUnidadMedida))
                .ReturnsAsync(unidadMedida);

            _lotesMPRepoMock
                .Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<System.Func<LotesMateriaPrimaEntity, bool>>>()))
                .Returns(lotes); // Returns both lotes (they match the filter)

            _lotesMPRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _movimientosMPRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var consumos = await _service.ConsumirLotesMateriaPrimaAsync(
                materiaPrimaId, cantidadRequerida, idUnidadMedida, idUsuario, observacion);

            // Assert
            // Debería consumir de ambos lotes: 20 del primero + 10 del segundo
            consumos.Should().HaveCount(2);
            consumos[0].CantidadConsumida.Should().Be(20); // primer lote: consume todo (20)
            consumos[1].CantidadConsumida.Should().Be(10); // segundo lote: consume lo que falta (10)

            // Verificar que se descontó stock de cada lote
            _lotesMPRepoMock.Verify(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(
                l => l.Id == 1 && l.CantidadDisponible == 0  // 20 - 20 = 0
            )), Times.Once);

            _lotesMPRepoMock.Verify(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(
                l => l.Id == 2 && l.CantidadDisponible == 10  // 20 - 10 = 10
            )), Times.Once);

            // Verificar que se crearon 2 movimientos de tipo Consumo
            _movimientosMPRepoMock.Verify(r => r.CreateAsync(It.Is<MovimientosMateriaPrimaEntity>(
                m => m.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Consumo
            )), Times.Exactly(2));

            // Verificar transacción: se inició, se hizo commit, no rollback
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar DisposeAsync
            _unitOfWorkMock.Verify(u => u.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task ConsumirLotesMateriaPrimaAsync_CuandoFallaUnLote_DeberiaRevertirTodo()
        {
            // Arrange
            var materiaPrimaId = 1;
            var cantidadRequerida = 30m;
            var idUnidadMedida = 1;
            var idUsuario = Guid.NewGuid();
            var observacion = "Consumo que falla";

            var materiaPrima = new MateriaPrimaEntity
            {
                Id = materiaPrimaId,
                Nombre = "Harina",
            };

            var unidadMedida = new UnidadMedidaEntity
            {
                Id = 1,
                Nombre = "Kilogramo",
                Abreviatura = "kg"
            };

            var lotes = new List<LotesMateriaPrimaEntity>
            {
                new()
                {
                    Id = 1,
                    IdMateria = materiaPrimaId,
                    CantidadDisponible = 20m,
                    Cantidad = 20m,
                    FechaVencimiento = DateTime.Now.AddDays(10),
                    Estado = 1,
                    IdUnidadMedida = 1
                },
                new()
                {
                    Id = 2,
                    IdMateria = materiaPrimaId,
                    CantidadDisponible = 20m,
                    Cantidad = 20m,
                    FechaVencimiento = DateTime.Now.AddDays(20),
                    Estado = 1,
                    IdUnidadMedida = 1
                }
            };

            var contadorLlamadas = 0;

            _unitOfWorkMock
                .Setup(u => u.HasActiveTransaction)
                .Returns(false);

            _materiaPrimaRepoMock
                .Setup(r => r.FindByIdAsync(materiaPrimaId))
                .ReturnsAsync(materiaPrima);

            _unidadMedidaRepoMock
                .Setup(r => r.FindByIdAsync(idUnidadMedida))
                .ReturnsAsync(unidadMedida);

            _lotesMPRepoMock
                .Setup(r => r.GetByFilter(It.IsAny<System.Linq.Expressions.Expression<System.Func<LotesMateriaPrimaEntity, bool>>>()))
                .Returns(lotes);

            // El primer lote se actualiza OK, el segundo lote falla
            _lotesMPRepoMock
                .Setup(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(l => l.Id == 1)))
                .Returns(Task.CompletedTask)
                .Callback(() => contadorLlamadas++);

            _lotesMPRepoMock
                .Setup(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(l => l.Id == 2)))
                .ThrowsAsync(new InvalidOperationException("Error al actualizar lote 2"));

            // Crear movimiento del primer lote: OK
            // El movimiento del segundo lote nunca se intenta porque falla antes
            _movimientosMPRepoMock
                .Setup(r => r.CreateAsync(It.Is<MovimientosMateriaPrimaEntity>(m => m.IdLoteMateria == 1)))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.ConsumirLotesMateriaPrimaAsync(
                materiaPrimaId, cantidadRequerida, idUnidadMedida, idUsuario, observacion);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            // Verificar que NO se hizo commit
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar que se hizo rollback (revertir TODO)
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verificar que DisposeAsync se llamó
            _unitOfWorkMock.Verify(u => u.DisposeAsync(), Times.Once);
        }
    }
}
