using DistribuidoraLaVilla.Application.Common;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class MovimientosMateriaPrimaServiceTests
    {
        private readonly Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>> _movimientosRepoMock;
        private readonly Mock<IGenericRepository<LotesMateriaPrimaEntity, int>> _lotesRepoMock;
        private readonly Mock<IGenericRepository<MateriaPrimaEntity, int>> _materiaPrimaRepoMock;
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepoMock;
        private readonly Mock<IGenericRepository<MarcasEntity, int>> _marcasRepoMock;
        private readonly Mock<IGenericRepository<ProveedoresEntity, Guid>> _proveedoresRepoMock;
        private readonly Mock<IGenericRepository<TipoMovimientoMateriaPrimaEntity, int>> _tipoMovimientoRepoMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly MovimientosMateriaPrimaService _service;

        public MovimientosMateriaPrimaServiceTests()
        {
            _movimientosRepoMock = new Mock<IGenericRepository<MovimientosMateriaPrimaEntity, int>>();
            _lotesRepoMock = new Mock<IGenericRepository<LotesMateriaPrimaEntity, int>>();
            _materiaPrimaRepoMock = new Mock<IGenericRepository<MateriaPrimaEntity, int>>();
            _unidadMedidaRepoMock = new Mock<IGenericRepository<UnidadMedidaEntity, int>>();
            _marcasRepoMock = new Mock<IGenericRepository<MarcasEntity, int>>();
            _proveedoresRepoMock = new Mock<IGenericRepository<ProveedoresEntity, Guid>>();
            _tipoMovimientoRepoMock = new Mock<IGenericRepository<TipoMovimientoMateriaPrimaEntity, int>>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _service = new MovimientosMateriaPrimaService(
                _movimientosRepoMock.Object,
                _lotesRepoMock.Object,
                _materiaPrimaRepoMock.Object,
                _unidadMedidaRepoMock.Object,
                _marcasRepoMock.Object,
                _proveedoresRepoMock.Object,
                _tipoMovimientoRepoMock.Object,
                _auditoriaServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        private static MovimientoMateriaPrimaDTO CrearDtoEntrada(int loteId = 1, decimal cantidad = 50)
        {
            return new MovimientoMateriaPrimaDTO
            {
                IdLoteMateria = loteId,
                IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Entrada,
                Cantidad = cantidad,
                IdUnidadMedida = 1,
                IdUsuario = Guid.NewGuid(),
                Observacion = "Entrada de prueba"
            };
        }

        private static MovimientoMateriaPrimaDTO CrearDtoConsumo(int loteId = 1, decimal cantidad = 30)
        {
            return new MovimientoMateriaPrimaDTO
            {
                IdLoteMateria = loteId,
                IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Consumo,
                Cantidad = cantidad,
                IdUnidadMedida = 1,
                IdUsuario = Guid.NewGuid(),
                Observacion = "Consumo de prueba"
            };
        }

        private static LotesMateriaPrimaEntity CrearLoteValido(int id = 1, decimal disponible = 100, decimal cantidad = 100)
        {
            return new LotesMateriaPrimaEntity
            {
                Id = id,
                IdMarca = 1,
                IdMateria = 1,
                IdProveedor = Guid.NewGuid(),
                FechaEntrada = DateTime.Now.AddDays(-10),
                FechaVencimiento = DateTime.Now.AddDays(20),
                Cantidad = cantidad,
                CantidadDisponible = disponible,
                CantidadInicial = cantidad,
                IdUnidadMedida = 1,
                CostoUnitario = 50m,
                CostoTotal = cantidad * 50m,
                Estado = 1
            };
        }

        #region CrearMovimientoMateriaPrimaAsync

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_DeberiaCrearMovimientoYActualizarStock()
        {
            // Arrange — entrada (suma stock)
            var dto = CrearDtoEntrada(loteId: 1, cantidad: 50);
            var lote = CrearLoteValido(id: 1, disponible: 100, cantidad: 100);
            var tipoMovimiento = new TipoMovimientoMateriaPrimaEntity { IdTipoMovimiento = 1, Nombre = "Entrada" };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync(lote);

            _tipoMovimientoRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdTipoMovimiento))
                .ReturnsAsync(tipoMovimiento);

            _lotesRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .Returns(Task.CompletedTask);

            _movimientosRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<MovimientosMateriaPrimaEntity>()))
                .Callback<MovimientosMateriaPrimaEntity>(m => m.Id = 99) // ID generado
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Movimiento creado correctamente");
            result.Data.Should().NotBeNull();

            // Verificar que se actualizó el stock del lote (100 + 50 = 150)
            _lotesRepoMock.Verify(r => r.UpdateAsync(It.Is<LotesMateriaPrimaEntity>(
                l => l.Id == 1 && l.CantidadDisponible == 150 && l.Cantidad == 150
            )), Times.Once);

            // Verificar que se creó el movimiento
            _movimientosRepoMock.Verify(r => r.CreateAsync(It.Is<MovimientosMateriaPrimaEntity>(
                m => m.IdLoteMateria == 1
                  && m.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Entrada
                  && m.Cantidad == 50
            )), Times.Once);

            // Verificar transacción exitosa
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            // Verificar stock calculado en response
            result.Data!.StockAnterior.Should().Be(100); // antes de sumar
            result.Data.StockNuevo.Should().Be(150);     // después de sumar
        }

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_CuandoLoteNoExiste_DeberiaRetornarError()
        {
            // Arrange
            var dto = CrearDtoEntrada();

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync((LotesMateriaPrimaEntity?)null); // lote no existe

            // Act
            var result = await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El lote de materia prima no existe");
            result.Data.Should().BeNull();

            // No debe iniciar transacción si falla validación previa
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_CuandoStockInsuficiente_DeberiaRetornarError()
        {
            // Arrange — consumo de 200 cuando solo hay 100 disponibles
            var dto = CrearDtoConsumo(loteId: 1, cantidad: 200);
            var lote = CrearLoteValido(id: 1, disponible: 100, cantidad: 100);
            var tipoMovimiento = new TipoMovimientoMateriaPrimaEntity { IdTipoMovimiento = 2, Nombre = "Consumo" };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync(lote);

            _tipoMovimientoRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdTipoMovimiento))
                .ReturnsAsync(tipoMovimiento);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Stock insuficiente");
            result.Message.Should().Contain("Disponible: 100");
            result.Data.Should().BeNull();

            // La transacción se inició pero se hizo rollback
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

            // No debe modificar el lote
            _lotesRepoMock.Verify(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()), Times.Never);
        }

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_CuandoTipoMovimientoNoExiste_DeberiaRetornarError()
        {
            // Arrange
            var dto = CrearDtoEntrada();
            var lote = CrearLoteValido();

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync(lote);

            // Tipo de movimiento no existe en BD
            _tipoMovimientoRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdTipoMovimiento))
                .ReturnsAsync((TipoMovimientoMateriaPrimaEntity?)null);

            // Act
            var result = await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El tipo de movimiento no existe");
            result.Data.Should().BeNull();

            // No debe iniciar transacción
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_CuandoCantidadEsCero_DeberiaRetornarError()
        {
            // Arrange
            var dto = CrearDtoEntrada(cantidad: 0);
            var lote = CrearLoteValido();
            var tipoMovimiento = new TipoMovimientoMateriaPrimaEntity { IdTipoMovimiento = 1, Nombre = "Entrada" };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync(lote);

            _tipoMovimientoRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdTipoMovimiento))
                .ReturnsAsync(tipoMovimiento);

            // Act
            var result = await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("La cantidad debe ser mayor a 0");
            result.Data.Should().BeNull();

            // No debe iniciar transacción
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CrearMovimientoMateriaPrimaAsync_CuandoExcepcion_DeberiaHacerRollback()
        {
            // Arrange
            var dto = CrearDtoEntrada();
            var lote = CrearLoteValido();
            var tipoMovimiento = new TipoMovimientoMateriaPrimaEntity { IdTipoMovimiento = 1, Nombre = "Entrada" };

            _lotesRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdLoteMateria))
                .ReturnsAsync(lote);

            _tipoMovimientoRepoMock
                .Setup(r => r.FindByIdAsync(dto.IdTipoMovimiento))
                .ReturnsAsync(tipoMovimiento);

            _lotesRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<LotesMateriaPrimaEntity>()))
                .ThrowsAsync(new InvalidOperationException("Error inesperado"));

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.CrearMovimientoMateriaPrimaAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.DisposeAsync(), Times.Once);
        }

        #endregion

        #region ObtenerMovimientosDetalleAsync

        [Fact]
        public async Task ObtenerMovimientosDetalleAsync_DeberiaCalcularStockAnteriorYStockNuevo()
        {
            // Arrange
            var loteId = 1;
            var materiaId = 1;
            var marcaId = 1;
            var proveedorId = Guid.NewGuid();
            var unidadId = 1;

            var movimientos = new List<MovimientosMateriaPrimaEntity>
            {
                new()
                {
                    Id = 1,
                    IdLoteMateria = loteId,
                    IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Entrada,
                    Fecha = DateTime.Now.AddDays(-5),
                    Cantidad = 100,
                    IdUnidadMedida = unidadId,
                    IdUsuario = Guid.NewGuid(),
                    Observacion = "Entrada inicial"
                },
                new()
                {
                    Id = 2,
                    IdLoteMateria = loteId,
                    IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Consumo,
                    Fecha = DateTime.Now.AddDays(-3),
                    Cantidad = 30,
                    IdUnidadMedida = unidadId,
                    IdUsuario = Guid.NewGuid(),
                    Observacion = "Consumo"
                },
                new()
                {
                    Id = 3,
                    IdLoteMateria = loteId,
                    IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Consumo,
                    Fecha = DateTime.Now.AddDays(-1),
                    Cantidad = 20,
                    IdUnidadMedida = unidadId,
                    IdUsuario = Guid.NewGuid(),
                    Observacion = "Otro consumo"
                }
            };

            var lotes = new List<LotesMateriaPrimaEntity>
            {
                new()
                {
                    Id = loteId,
                    IdMarca = marcaId,
                    IdMateria = materiaId,
                    IdProveedor = proveedorId,
                    FechaEntrada = DateTime.Now.AddDays(-10),
                    FechaVencimiento = DateTime.Now.AddDays(20),
                    Cantidad = 100,
                    CantidadDisponible = 50,
                    CantidadInicial = 100,
                    IdUnidadMedida = unidadId,
                    CostoUnitario = 50m,
                    CostoTotal = 5000m,
                    Estado = 1
                }
            };

            var materias = new List<MateriaPrimaEntity>
            {
                new() { Id = materiaId, Nombre = "Harina" }
            };

            var marcas = new List<MarcasEntity>
            {
                new() { IdMarca = marcaId, Nombre = "Marca Test" }
            };

            var proveedores = new List<ProveedoresEntity>
            {
                new() { IdProveedor = proveedorId, Nombre = "Proveedor Test" }
            };

            var unidades = new List<UnidadMedidaEntity>
            {
                new() { Id = unidadId, Nombre = "Kilogramo", Abreviatura = "kg" }
            };

            _movimientosRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(movimientos);

            _lotesRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(lotes);

            _materiaPrimaRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(materias);

            _marcasRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(marcas);

            _proveedoresRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(proveedores);

            _unidadMedidaRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(unidades);

            // Act
            var resultado = await _service.ObtenerMovimientosDetalleAsync();

            // Assert
            resultado.Should().HaveCount(3);

            // Verificar orden: descendente por fecha (el más reciente primero)
            resultado[0].Id.Should().Be(3); // Fecha más reciente
            resultado[1].Id.Should().Be(2);
            resultado[2].Id.Should().Be(1); // Fecha más antigua

            // Verificar cálculo de stock:
            // Mov 1 (Entrada 100): anterior=0, nuevo=100
            var mov1 = resultado.Single(m => m.Id == 1);
            mov1.StockAnterior.Should().Be(0);
            mov1.StockNuevo.Should().Be(100);
            mov1.TipoMovimientoNombre.Should().Be("Entrada");

            // Mov 2 (Consumo 30): anterior=100, nuevo=70
            var mov2 = resultado.Single(m => m.Id == 2);
            mov2.StockAnterior.Should().Be(100);
            mov2.StockNuevo.Should().Be(70);
            mov2.TipoMovimientoNombre.Should().Be("Consumo");

            // Mov 3 (Consumo 20): anterior=70, nuevo=50
            var mov3 = resultado.Single(m => m.Id == 3);
            mov3.StockAnterior.Should().Be(70);
            mov3.StockNuevo.Should().Be(50);
            mov3.TipoMovimientoNombre.Should().Be("Consumo");

            // Verificar nombres resueltos
            mov1.NombreLote.Should().Be("Harina - Marca Test");
            mov1.NombreUnidadMedida.Should().Be("Kilogramo");
            mov1.SimboloUnidadMedida.Should().Be("kg");
        }

        #endregion
    }
}
