using System.Linq.Expressions;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services.Facturacion;
using DistribuidoraLaVilla.Domain.DTOS.Facturacion;
using DistribuidoraLaVilla.Domain.DTOS.Inventario;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DistribuidoraLaVilla.Api.Tests.Tests
{
    public class FacturaServiceTests
    {
        private readonly Mock<IInventarioService> _inventarioServiceMock;
        private readonly Mock<IGenericRepository<FacturaEntity, int>> _facturaRepositoryMock;
        private readonly Mock<IGenericRepository<DetalleFacturaEntity, int>> _detalleRepositoryMock;
        private readonly Mock<IGenericRepository<ClientesEntity, Guid>> _clienteRepositoryMock;
        private readonly Mock<IGenericRepository<ProductosEntity, int>> _productoRepositoryMock;
        private readonly Mock<IGenericRepository<UnidadMedidaEntity, int>> _unidadMedidaRepositoryMock;
        private readonly Mock<IGenericRepository<UsuariosEntity, Guid>> _usuariosRepositoryMock;
        private readonly Mock<IGenericRepository<CuentasCobrarEntity, int>> _cxcRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICuentasCobrarService> _cuentasCobrarServiceMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Mock<ICajaService> _cajaServiceMock;

        private readonly FacturaService _service;

        private static readonly Guid ClienteId = Guid.NewGuid();
        private static readonly Guid UsuarioId = Guid.NewGuid();

        private readonly List<DetalleFacturaEntity> _detallesCreados = new();
        private FacturaEntity? _facturaCreada;

        public FacturaServiceTests()
        {
            _inventarioServiceMock = new Mock<IInventarioService>();
            _facturaRepositoryMock = new Mock<IGenericRepository<FacturaEntity, int>>();
            _detalleRepositoryMock = new Mock<IGenericRepository<DetalleFacturaEntity, int>>();
            _clienteRepositoryMock = new Mock<IGenericRepository<ClientesEntity, Guid>>();
            _productoRepositoryMock = new Mock<IGenericRepository<ProductosEntity, int>>();
            _unidadMedidaRepositoryMock = new Mock<IGenericRepository<UnidadMedidaEntity, int>>();
            _usuariosRepositoryMock = new Mock<IGenericRepository<UsuariosEntity, Guid>>();
            _cxcRepositoryMock = new Mock<IGenericRepository<CuentasCobrarEntity, int>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cuentasCobrarServiceMock = new Mock<ICuentasCobrarService>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _cajaServiceMock = new Mock<ICajaService>();

            _service = new FacturaService(
                _inventarioServiceMock.Object,
                _facturaRepositoryMock.Object,
                _detalleRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _unidadMedidaRepositoryMock.Object,
                _usuariosRepositoryMock.Object,
                _cxcRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _cuentasCobrarServiceMock.Object,
                _auditoriaServiceMock.Object,
                _cajaServiceMock.Object);
        }

        // ── Helpers ──

        private static ProductosEntity CreateProductoUnitario(decimal precioUnitario = 800m, int id = 1) => new()
        {
            Id = id,
            Nombre = "Producto Unitario",
            PrecioUnitario = precioUnitario,
            VentaPorPeso = false,
            PrecioPorKilo = null,
            Estado = 1
        };

        private static ProductosEntity CreateProductoPeso(decimal? precioPorKilo = 500m, int id = 1) => new()
        {
            Id = id,
            Nombre = "Producto Peso",
            PrecioUnitario = 0m,
            VentaPorPeso = true,
            PrecioPorKilo = precioPorKilo,
            Estado = 1
        };

        private static CrearFacturaDTO CreateFacturaContado(decimal precio, decimal cantidad = 1m, bool esVentaPorPeso = false, int idProducto = 1) => new()
        {
            IdCliente = ClienteId,
            IdUsuario = UsuarioId,
            FormaPago = 1,
            MetodoPago = 1,
            Detalles = new List<CrearDetalleFacturaDTO>
            {
                new()
                {
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    IdUnidadMedida = esVentaPorPeso ? 1 : 2,
                    Precio = precio,
                    EsVentaPorPeso = esVentaPorPeso
                }
            }
        };

        private static CrearFacturaCreditoDTO CreateFacturaCredito(decimal precio, decimal cantidad = 1m, bool esVentaPorPeso = false) => new()
        {
            IdCliente = ClienteId,
            IdUsuario = UsuarioId,
            FormaPago = 1,
            MetodoPago = 2,
            Detalles = new List<CrearDetalleFacturaDTO>
            {
                new()
                {
                    IdProducto = 1,
                    Cantidad = cantidad,
                    IdUnidadMedida = esVentaPorPeso ? 1 : 2,
                    Precio = precio,
                    EsVentaPorPeso = esVentaPorPeso
                }
            }
        };

        private void SetupCliente(decimal? limiteCredito = 10000m)
        {
            var cliente = new ClientesEntity
            {
                IdCliente = ClienteId,
                Nombre = "Cliente Test",
                Documento = "12345678",
                Email = "cliente@test.com",
                LimiteCredito = limiteCredito,
                DiasCredito = 30,
                Estado = 1
            };
            _clienteRepositoryMock.Setup(r => r.FindByIdAsync(ClienteId)).ReturnsAsync(cliente);
        }

        private void SetupProducto(ProductosEntity producto)
        {
            _productoRepositoryMock.Setup(r => r.FindByIdAsync(producto.Id)).ReturnsAsync(producto);
        }

        private void SetupConsumo(decimal cantidadConsumida = 1m, int idLote = 10)
        {
            _inventarioServiceMock
                .Setup(i => i.ConsumirLotesProductoAsync(
                    It.IsAny<int>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<decimal?>()))
                .ReturnsAsync(new List<ConsumoProductoDTO>
                {
                    new()
                    {
                        IdProducto = 1,
                        CantidadConsumida = cantidadConsumida,
                        IdLote = idLote
                    }
                });
        }

        private void SetupFacturaCreate()
        {
            _facturaRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<FacturaEntity>()))
                .Callback<FacturaEntity>(e =>
                {
                    e.Id = 1;
                    _facturaCreada = e;
                })
                .Returns(Task.CompletedTask);
        }

        private void SetupDetalleCreate()
        {
            _detalleRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<DetalleFacturaEntity>()))
                .Callback<DetalleFacturaEntity>(e => _detallesCreados.Add(e))
                .Returns(Task.CompletedTask);
        }

        /// <summary>
        /// GATE wiring: GenerarTicketAsync post-create dependencies
        /// (factura re-query, usuario cajero, unidad de medida, detalle list).
        /// Required on every success-path test.
        /// </summary>
        private void SetupTicketWiring(int idUnidadMedida)
        {
            _facturaRepositoryMock
                .Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(() => _facturaCreada!);

            _usuariosRepositoryMock
                .Setup(r => r.FindByIdAsync(UsuarioId))
                .ReturnsAsync(new UsuariosEntity { Id = UsuarioId, Nombre = "Cajero Test", Email = "cajero@test.com" });

            _unidadMedidaRepositoryMock
                .Setup(r => r.FindByIdAsync(idUnidadMedida))
                .ReturnsAsync(new UnidadMedidaEntity { Id = idUnidadMedida, Nombre = "Unidad", Abreviatura = "u", Tipo = 1 });

            _detalleRepositoryMock
                .Setup(r => r.GetByFilter(It.IsAny<Expression<Func<DetalleFacturaEntity, bool>>>()))
                .Returns(() => _detallesCreados.ToList());
        }

        // ── Guard: precio <= 0 rechazado para TODAS las líneas ──

        [Fact]
        public async Task CrearFacturaContado_CuandoPrecioEsCeroParaProductoUnidad_DeberiaRechazarConValidacion()
        {
            // Arrange
            SetupCliente();
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            SetupFacturaCreate();
            SetupDetalleCreate();
            var solicitud = CreateFacturaContado(precio: 0m);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("El precio debe ser mayor a 0");
            _facturaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FacturaEntity>()), Times.Never);
            _detalleRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<DetalleFacturaEntity>()), Times.Never);
            _inventarioServiceMock.Verify(
                i => i.ConsumirLotesProductoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(),
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<decimal?>()),
                Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CrearFacturaContado_CuandoPrecioEsNegativoParaProductoUnidad_DeberiaRechazarConValidacion()
        {
            // Arrange
            SetupCliente();
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            SetupFacturaCreate();
            SetupDetalleCreate();
            var solicitud = CreateFacturaContado(precio: -100m);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("El precio debe ser mayor a 0");
            _facturaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FacturaEntity>()), Times.Never);
            _detalleRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<DetalleFacturaEntity>()), Times.Never);
        }

        [Fact]
        public async Task CrearFacturaContado_CuandoPrecioEsCeroParaProductoPeso_DeberiaRechazarConValidacion()
        {
            // Arrange (regresión del guard anterior de solo-peso)
            SetupCliente();
            SetupProducto(CreateProductoPeso(precioPorKilo: 500m));
            SetupFacturaCreate();
            SetupDetalleCreate();
            var solicitud = CreateFacturaContado(precio: 0m, esVentaPorPeso: true);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("El precio debe ser mayor a 0");
            _facturaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FacturaEntity>()), Times.Never);
        }

        // ── Camino de éxito ──

        [Fact]
        public async Task CrearFacturaContado_ConPrecioValidoParaProductoPeso_DeberiaCrearFacturaYCalcularSubtotal()
        {
            // Arrange
            SetupCliente();
            SetupProducto(CreateProductoPeso(precioPorKilo: 500m));
            SetupConsumo(cantidadConsumida: 2m, idLote: 10);
            SetupFacturaCreate();
            SetupDetalleCreate();
            SetupTicketWiring(idUnidadMedida: 1);
            var solicitud = CreateFacturaContado(precio: 450m, cantidad: 2m, esVentaPorPeso: true);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeTrue();
            result.Total.Should().Be(900m); // 2 kg x 450 = 900
            _detalleRepositoryMock.Verify(r => r.CreateAsync(It.Is<DetalleFacturaEntity>(d =>
                d.Precio == 450m &&
                d.Subtotal == 900m &&
                d.EsVentaPorPeso == true)), Times.Once);
            _facturaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FacturaEntity>()), Times.Once);
        }

        [Fact]
        public async Task CrearFacturaContado_ConPrecioValidoParaProductoUnidad_DeberiaCrearFactura()
        {
            // Arrange
            SetupCliente();
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            SetupConsumo(cantidadConsumida: 3m, idLote: 5);
            SetupFacturaCreate();
            SetupDetalleCreate();
            SetupTicketWiring(idUnidadMedida: 2);
            var solicitud = CreateFacturaContado(precio: 750m, cantidad: 3m);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeTrue();
            result.Total.Should().Be(2250m); // 3 x 750
        }

        // ── Crédito: guard en el loop de estimación ──

        [Fact]
        public async Task CrearFacturaCredito_CuandoPrecioEsInvalido_DeberiaRechazarEnLoopDeEstimacionSinCrearCxC()
        {
            // Arrange: el total estimado (0) pasaría el límite de crédito, así que el
            // rechazo solo puede venir del guard del loop de estimación.
            SetupCliente(limiteCredito: 10000m);
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            var solicitud = CreateFacturaCredito(precio: 0m);

            // Act
            var result = await _service.CrearFacturaCreditoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("El precio debe ser mayor a 0");
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _facturaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FacturaEntity>()), Times.Never);
            _cxcRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<CuentasCobrarEntity>()), Times.Never);
        }

        // ── precio_original ──

        [Fact]
        public async Task CrearFacturaContado_LineaUnidadVendidaConDescuento_DeberiaPersistirPrecioOriginalDelCatalogo()
        {
            // Arrange: unitario 800 vendido a 750 (FAC-5)
            SetupCliente();
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            SetupConsumo(cantidadConsumida: 1m, idLote: 1);
            SetupFacturaCreate();
            SetupDetalleCreate();
            SetupTicketWiring(idUnidadMedida: 2);
            var solicitud = CreateFacturaContado(precio: 750m, cantidad: 1m);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeTrue();
            var detalle = _detallesCreados.Should().ContainSingle().Subject;
            detalle.Precio.Should().Be(750m);
            detalle.PrecioOriginal.Should().Be(800m);
        }

        [Fact]
        public async Task CrearFacturaContado_LineaPesoVendidaConDescuento_DeberiaPersistirPrecioOriginalPorKilo()
        {
            // Arrange: por kilo 500 vendido a 450 (FAC-5)
            SetupCliente();
            SetupProducto(CreateProductoPeso(precioPorKilo: 500m));
            SetupConsumo(cantidadConsumida: 2.5m, idLote: 7);
            SetupFacturaCreate();
            SetupDetalleCreate();
            SetupTicketWiring(idUnidadMedida: 1);
            var solicitud = CreateFacturaContado(precio: 450m, cantidad: 2.5m, esVentaPorPeso: true);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeTrue();
            var detalle = _detallesCreados.Should().ContainSingle().Subject;
            detalle.Precio.Should().Be(450m);
            detalle.PrecioOriginal.Should().Be(500m);
        }

        [Fact]
        public async Task CrearFacturaContado_ProductoPesoConPrecioPorKiloNulo_DeberiaPersistirPrecioOriginalNulo()
        {
            // Arrange: peso sin PrecioPorKilo -> nada que registrar (NULL)
            SetupCliente();
            SetupProducto(CreateProductoPeso(precioPorKilo: null));
            SetupConsumo(cantidadConsumida: 1m, idLote: 3);
            SetupFacturaCreate();
            SetupDetalleCreate();
            SetupTicketWiring(idUnidadMedida: 1);
            var solicitud = CreateFacturaContado(precio: 450m, cantidad: 1m, esVentaPorPeso: true);

            // Act
            var result = await _service.CrearFacturaContadoAsync(solicitud);

            // Assert
            result.Exitoso.Should().BeTrue();
            var detalle = _detallesCreados.Should().ContainSingle().Subject;
            detalle.PrecioOriginal.Should().BeNull();
        }

        // ── Ticketera POS ──

        [Fact]
        public async Task CrearTicketera_CuandoPrecioEsInvalido_DeberiaLanzarInvalidOperationException()
        {
            // Arrange
            SetupCliente();
            SetupProducto(CreateProductoUnitario(precioUnitario: 800m));
            var dto = new CrearTicketeraDTO
            {
                IdCliente = ClienteId,
                FormaPago = 1,
                Detalles = new List<DetalleTicketeraDTO>
                {
                    new()
                    {
                        IdProducto = 1,
                        Cantidad = 1m,
                        Precio = 0m,
                        IdUnidadMedida = 2,
                        EsVentaPorPeso = false
                    }
                }
            };

            // Act
            var act = async () => await _service.CrearTicketeraAsync(dto, UsuarioId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El precio debe ser mayor a 0");
        }
    }
}
