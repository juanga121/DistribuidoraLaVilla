using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Domain.DTOS.Compras;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Compras;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraLaVilla.Application.Services.Compras
{
    public class OrdenCompraService(
        IGenericRepository<OrdenCompraEntity, int> ordenCompraRepository,
        IGenericRepository<DetalleCompraEntity, int> detalleRepository,
        IGenericRepository<ProveedoresEntity, Guid> proveedorRepository,
        IGenericRepository<ProductosEntity, int> productoRepository,
        IGenericRepository<MarcasEntity, int> marcaRepository,
        IGenericRepository<RecepcionCompraEntity, int> recepcionCompraRepository,
        IGenericRepository<CuentasPagarEntity, int> cuentasPagarRepository,
        IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
        IGenericRepository<MovimientosProductosEntity, int> movimientosProductosRepository,
        IGenericRepository<CajaAperturaEntity, int> cajaAperturaRepository,
        IGenericRepository<CajaMovimientoEntity, int> cajaMovimientoRepository,
        IUnitOfWork unitOfWork,
        IGenericRepository<UnidadMedidaEntity, int>? unidadMedidaRepository = null,
        IGenericRepository<UsuariosEntity, Guid>? usuariosRepository = null)
    {
        private readonly IGenericRepository<OrdenCompraEntity, int> _ordenCompraRepository = ordenCompraRepository;
        private readonly IGenericRepository<DetalleCompraEntity, int> _detalleRepository = detalleRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedorRepository = proveedorRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productoRepository = productoRepository;
        private readonly IGenericRepository<MarcasEntity, int> _marcaRepository = marcaRepository;
        private readonly IGenericRepository<RecepcionCompraEntity, int> _recepcionCompraRepository = recepcionCompraRepository;
        private readonly IGenericRepository<CuentasPagarEntity, int> _cuentasPagarRepository = cuentasPagarRepository;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository = lotesProductosRepository;
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosProductosRepository = movimientosProductosRepository;
        private readonly IGenericRepository<CajaAperturaEntity, int> _cajaAperturaRepository = cajaAperturaRepository;
        private readonly IGenericRepository<CajaMovimientoEntity, int> _cajaMovimientoRepository = cajaMovimientoRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int>? _unidadMedidaRepository = unidadMedidaRepository;
        private readonly IGenericRepository<UsuariosEntity, Guid>? _usuariosRepository = usuariosRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CrearOrdenCompraDTOValidator _validator = new();
        private readonly RegistrarRecepcionCompraDTOValidator _recepcionValidator = new();

        public async Task<OrdenCompraResponseDTO> CrearOrdenCompraAsync(CrearOrdenCompraDTO dto, Guid idUsuario)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            var proveedor = await _proveedorRepository.FindByIdAsync(dto.IdProveedor)
                ?? throw new KeyNotFoundException($"No se encontró el proveedor con ID {dto.IdProveedor}");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                decimal subtotalTotal = 0;
                foreach (var det in dto.Detalles)
                {
                    subtotalTotal += det.Cantidad * det.PrecioUnitario;
                }

                var orden = new OrdenCompraEntity
                {
                    IdProveedor = dto.IdProveedor,
                    FechaEmision = dto.FechaEmision,
                    Estado = (int)EstadoCompraEnum.Pendiente,
                    Subtotal = subtotalTotal,
                    Descuento = 0,
                    Impuesto = 0,
                    Total = subtotalTotal,
                    Observaciones = dto.Observaciones,
                    FechaCreacion = DateTime.Now
                };

                await _ordenCompraRepository.CreateAsync(orden);

                foreach (var det in dto.Detalles)
                {
                    var producto = await _productoRepository.FindByIdAsync(det.IdProducto)
                        ?? throw new KeyNotFoundException($"No se encontró el producto con ID {det.IdProducto}");

                    var detalle = new DetalleCompraEntity
                    {
                        IdOrdenCompra = orden.Id,
                        IdProducto = det.IdProducto,
                        Cantidad = det.Cantidad,
                        PrecioUnitario = det.PrecioUnitario,
                        Subtotal = det.Cantidad * det.PrecioUnitario
                    };

                    await _detalleRepository.CreateAsync(detalle);
                }

                await _unitOfWork.CommitAsync();

                return MapToResponse(orden, proveedor.Nombre);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrdenCompraResponseDTO>> ObtenerOrdenesCompraAsync()
        {
            var ordenes = await _ordenCompraRepository.GetAllAsync();
            var resultado = new List<OrdenCompraResponseDTO>();

            foreach (var orden in ordenes)
            {
                var nombreProveedor = await ObtenerNombreProveedorAsync(orden.IdProveedor);
                resultado.Add(MapToResponse(orden, nombreProveedor));
            }

            return resultado;
        }

        public async Task<OrdenCompraResponseDTO?> ObtenerOrdenCompraPorIdAsync(int id)
        {
            var orden = await _ordenCompraRepository.FindByIdAsync(id);
            if (orden == null) return null;

            var nombreProveedor = await ObtenerNombreProveedorAsync(orden.IdProveedor);
            var response = MapToResponse(orden, nombreProveedor);

            var detallesEntity = _detalleRepository.GetByFilter(d => d.IdOrdenCompra == id);
            var detallesDTO = new List<DetalleCompraResponseDTO>();

            foreach (var det in detallesEntity)
            {
                var producto = await _productoRepository.FindByIdAsync(det.IdProducto);
                var nombreProducto = await ObtenerNombreProductoAsync(det.IdProducto);
                detallesDTO.Add(new DetalleCompraResponseDTO
                {
                    Id = det.Id,
                    IdProducto = det.IdProducto,
                    NombreProducto = nombreProducto,
                    Cantidad = det.Cantidad,
                    PrecioUnitario = det.PrecioUnitario,
                    Subtotal = det.Subtotal,
                    VentaPorPeso = producto?.VentaPorPeso ?? false,
                    PesoPorUnidad = producto?.PesoPorUnidad
                });
            }

            response.Detalles = detallesDTO;
            return response;
        }

        /// <summary>
        /// Genera el comprobante de recepción de compra de una orden (CA06).
        /// La recepción es 1:1 con la orden, por lo que se busca por IdOrdenCompra.
        /// </summary>
        public async Task<ComprobanteCompraDTO?> ObtenerComprobanteRecepcionAsync(int idOrdenCompra)
        {
            var recepcion = _recepcionCompraRepository
                .GetByFilter(r => r.IdOrdenCompra == idOrdenCompra)
                .FirstOrDefault();

            if (recepcion == null)
            {
                throw new KeyNotFoundException(
                    $"No se encontró una recepción de compra para la orden con ID {idOrdenCompra}");
            }

            var proveedor = await _proveedorRepository.FindByIdAsync(recepcion.IdProveedor);

            string? usuarioNombre = null;
            if (recepcion.IdUsuario.HasValue && _usuariosRepository != null)
            {
                var usuario = await _usuariosRepository.FindByIdAsync(recepcion.IdUsuario.Value);
                usuarioNombre = usuario?.Nombre ?? usuario?.Email;
            }

            var lotes = _lotesProductosRepository
                .GetByFilter(l => l.IdRecepcionCompra == recepcion.IdRecepcionCompra)
                .ToList();

            var detalles = new List<ComprobanteCompraDetalleDTO>();
            decimal total = 0;

            foreach (var lote in lotes)
            {
                var producto = await _productoRepository.FindByIdAsync(lote.IdProducto);
                var unidad = _unidadMedidaRepository != null
                    ? await _unidadMedidaRepository.FindByIdAsync(lote.IdUnidadMedida)
                    : null;

                var subtotal = lote.PrecioTotal;
                total += subtotal;

                detalles.Add(new ComprobanteCompraDetalleDTO
                {
                    ProductoNombre = producto?.Nombre ?? $"Producto ID {lote.IdProducto}",
                    Cantidad = lote.CantidadUnidades,
                    UnidadMedida = unidad?.Abreviatura ?? "",
                    PrecioUnitario = lote.PrecioUnitario,
                    Subtotal = subtotal
                });
            }

            return new ComprobanteCompraDTO
            {
                IdRecepcionCompra = recepcion.IdRecepcionCompra,
                IdOrdenCompra = recepcion.IdOrdenCompra,
                NumeroFacturaProveedor = recepcion.NumeroFacturaProveedor,
                FechaFactura = recepcion.FechaFactura,
                FechaRecepcion = recepcion.FechaRecepcion,
                ProveedorNombre = proveedor?.Nombre,
                FormaPago = recepcion.FormaPago,
                FormaPagoTexto = recepcion.FormaPago == (int)FormaPago.Contado ? "Contado" : "Crédito",
                UsuarioNombre = usuarioNombre,
                Detalles = detalles,
                Total = total
            };
        }

        public async Task ActualizarEstadoOrdenAsync(int id, int nuevoEstado, Guid idUsuario)
        {
            var orden = await _ordenCompraRepository.FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró la orden de compra con ID {id}");

            if (nuevoEstado == (int)EstadoCompraEnum.Recibida)
            {
                throw new InvalidOperationException("La recepción de compra debe registrarse con el flujo de recepción");
            }

            ValidarTransicionEstado(orden.Estado, nuevoEstado);

            orden.Estado = nuevoEstado;
            orden.FechaActualizacion = DateTime.Now;

            if (nuevoEstado == (int)EstadoCompraEnum.Recibida)
            {
                orden.FechaRecepcion = DateTime.Now;
            }

            await _ordenCompraRepository.UpdateAsync(orden);
        }

        public async Task RegistrarRecepcionCompraAsync(int idOrdenCompra, RegistrarRecepcionCompraDTO dto, Guid idUsuario)
        {
            var validationResult = await _recepcionValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            // Backward compatible: a missing FormaPago (old frontend) defaults to Crédito,
            // which matches the historical behavior of always creating CxP.
            var formaPago = dto.FormaPago ?? (int)FormaPago.Credito;
            if (formaPago != (int)FormaPago.Contado && formaPago != (int)FormaPago.Credito)
            {
                throw new ValidationException($"La forma de pago debe ser Contado (1) o Crédito (2). Valor recibido: {formaPago}");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orden = await _ordenCompraRepository.FindByIdAsync(idOrdenCompra)
                    ?? throw new KeyNotFoundException($"No se encontró la orden de compra con ID {idOrdenCompra}");

                if (orden.Estado != (int)EstadoCompraEnum.Aprobada)
                {
                    throw new InvalidOperationException("Solo se pueden recibir órdenes de compra aprobadas");
                }

                var recepcionExistente = _recepcionCompraRepository.GetByFilter(r => r.IdOrdenCompra == idOrdenCompra).FirstOrDefault();
                if (recepcionExistente != null)
                {
                    throw new InvalidOperationException("La orden de compra ya fue recibida");
                }

                var proveedor = await _proveedorRepository.FindByIdAsync(orden.IdProveedor)
                    ?? throw new KeyNotFoundException($"No se encontró el proveedor con ID {orden.IdProveedor}");

                var marca = await _marcaRepository.FindByIdAsync(dto.IdMarca)
                    ?? throw new KeyNotFoundException($"No se encontró la marca con ID {dto.IdMarca}");

                if (marca.Estado != 1)
                {
                    throw new InvalidOperationException($"La marca con ID {dto.IdMarca} no está disponible");
                }

                var recepcion = new RecepcionCompraEntity
                {
                    IdOrdenCompra = orden.Id,
                    IdProveedor = orden.IdProveedor,
                    NumeroFacturaProveedor = dto.NumeroFacturaProveedor.Trim(),
                    FechaFactura = dto.FechaFactura,
                    FechaRecepcion = dto.FechaRecepcion,
                    FechaVencimiento = dto.FechaVencimiento,
                    FechaVencimientoLotes = dto.FechaVencimientoLotes,
                    IdMarca = dto.IdMarca,
                    MontoTotal = orden.Total,
                    FormaPago = formaPago,
                    Observaciones = dto.Observaciones,
                    Estado = 1,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
                };

                await _recepcionCompraRepository.CreateAsync(recepcion);

                var detallesOrden = _detalleRepository.GetByFilter(d => d.IdOrdenCompra == idOrdenCompra);
                if (detallesOrden.Count == 0)
                {
                    throw new InvalidOperationException("La orden de compra no tiene detalles para recepcionar");
                }

                if (dto.Detalles.Count == 0)
                {
                    throw new InvalidOperationException("La recepción debe incluir detalles explícitos por producto");
                }

                var lineasRecepcion = dto.Detalles;
                decimal totalRecepcion = 0m;

                foreach (var detalle in lineasRecepcion)
                {
                    var producto = await _productoRepository.FindByIdAsync(detalle.IdProducto)
                        ?? throw new KeyNotFoundException($"No se encontró el producto con ID {detalle.IdProducto}");

                    await CrearLoteYMovimientoAsync(orden, detalle, producto, recepcion, idUsuario);
                    totalRecepcion += detalle.CantidadUnidades * detalle.PrecioUnitario;
                }

                var cuentaPagar = new CuentasPagarEntity
                {
                    IdProveedor = orden.IdProveedor,
                    IdOrdenCompra = orden.Id,
                    MontoTotal = totalRecepcion,
                    SaldoPendiente = totalRecepcion,
                    Descripcion = $"Factura proveedor {recepcion.NumeroFacturaProveedor} - OC #{orden.Id}",
                    FechaVencimiento = dto.FechaVencimiento,
                    Estado = 1,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
                };

                if (formaPago == (int)FormaPago.Contado)
                {
                    // Cash purchase: requires an open cash register. The egreso is persisted
                    // inside the same transaction as the reception, so a failure anywhere
                    // rolls back the whole operation.
                    var cajaAbierta = await _cajaAperturaRepository.GetQueryable()
                        .Where(c => c.Estado == (int)EstadoCajaEnum.Abierta)
                        .OrderByDescending(c => c.FechaApertura)
                        .FirstOrDefaultAsync();

                    if (cajaAbierta == null)
                    {
                        throw new InvalidOperationException(
                            "No hay una caja abierta: para registrar una compra de contado debe existir una caja abierta. Abra la caja o registre la compra a crédito");
                    }

                    var movimientoCaja = new CajaMovimientoEntity
                    {
                        IdApertura = cajaAbierta.Id,
                        TipoMovimiento = (int)TipoMovimientoCajaEnum.Egreso,
                        Concepto = $"Pago contado OC #{orden.Id} - Factura {recepcion.NumeroFacturaProveedor}",
                        Monto = totalRecepcion,
                        Fecha = DateTime.Now,
                        IdUsuario = idUsuario,
                        MetodoPago = null
                    };

                    await _cajaMovimientoRepository.CreateAsync(movimientoCaja);
                }
                else
                {
                    await _cuentasPagarRepository.CreateAsync(cuentaPagar);
                }

                orden.Estado = (int)EstadoCompraEnum.Recibida;
                orden.FechaRecepcion = dto.FechaRecepcion;
                orden.Subtotal = totalRecepcion;
                orden.Total = totalRecepcion;
                orden.FechaActualizacion = DateTime.Now;

                await _ordenCompraRepository.UpdateAsync(orden);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private static void ValidarTransicionEstado(int desde, int hacia)
        {
            bool valida = (desde, hacia) switch
            {
                (1, 2) => true,
                (1, 4) => true,
                (2, 4) => true,
                _ => false
            };

            if (!valida)
            {
                throw new InvalidOperationException(
                    $"Transición de estado inválida: {desde} → {hacia}");
            }
        }

        private async Task<string?> ObtenerNombreProveedorAsync(Guid idProveedor)
        {
            var proveedor = await _proveedorRepository.FindByIdAsync(idProveedor);
            return proveedor?.Nombre;
        }

        private async Task<string?> ObtenerNombreProductoAsync(int idProducto)
        {
            var producto = await _productoRepository.FindByIdAsync(idProducto);
            return producto?.Nombre;
        }

        private async Task CrearLoteYMovimientoAsync(
            OrdenCompraEntity orden,
            RegistrarRecepcionCompraDetalleDTO detalle,
            ProductosEntity producto,
            RecepcionCompraEntity recepcion,
            Guid idUsuario)
        {
            var ventaPorPeso = producto.VentaPorPeso;
            var pesoPorUnidad = producto.PesoPorUnidad;
            if (ventaPorPeso && (!pesoPorUnidad.HasValue || pesoPorUnidad <= 0))
            {
                throw new InvalidOperationException(
                    $"El producto '{producto.Nombre}' está habilitado para venta por peso pero no tiene peso por unidad definido");
            }

            var idUnidadMedida = ventaPorPeso ? 1 : 2;
            if (detalle.IdUnidadMedida > 0 && detalle.IdUnidadMedida != idUnidadMedida)
            {
                throw new InvalidOperationException(
                    $"La unidad de medida del detalle no coincide con el modo del producto '{producto.Nombre}'");
            }

            if (detalle.PrecioUnitario <= 0 || detalle.PrecioKilo <= 0)
            {
                throw new InvalidOperationException(
                    $"La recepción del producto '{producto.Nombre}' debe incluir ambos costos positivos");
            }

            // Reception is a data capture step: the operator reports the weight that physically
            // arrived, so the lot is persisted with that submitted weight instead of one
            // recomputed from the product's PesoPorUnidad. As a result the lot carries its own
            // weight ratio (PesoTotal / CantidadUnidades), which can legitimately differ from
            // the product's nominal ratio. Consumption conversion in InventarioService still
            // uses the product ratio and will be revisited to consume this per-lot truth.
            var cantidadDisponible = detalle.CantidadUnidades;
            var pesoDisponible = detalle.PesoTotal;
            var cantidadUnidades = Math.Max(1, (int)Math.Round(detalle.CantidadUnidades, MidpointRounding.AwayFromZero));

            var lote = new LotesProductosEntity
            {
                IdProducto = detalle.IdProducto,
                IdProveedor = orden.IdProveedor,
                FechaEntrada = recepcion.FechaRecepcion,
                FechaVencimiento = detalle.FechaVencimientoLote,
                CantidadUnidades = cantidadUnidades,
                PesoTotal = pesoDisponible,
                IdUnidadMedida = idUnidadMedida,
                PrecioUnitario = detalle.PrecioUnitario,
                PrecioKilo = detalle.PrecioKilo,
                PrecioTotal = detalle.CantidadUnidades * detalle.PrecioUnitario,
                IdMarca = recepcion.IdMarca,
                IdRecepcionCompra = recepcion.IdRecepcionCompra,
                CantidadInicial = cantidadDisponible,
                CantidadDisponible = cantidadDisponible,
                PesoDisponible = pesoDisponible,
                Estado = 1
            };

            await _lotesProductosRepository.CreateAsync(lote);

            var movimiento = new MovimientosProductosEntity
            {
                    IdLoteProducto = lote.Id,
                    TipoMovimiento = (int)TipoMovimientoProducto.Entrada,
                    FechaMovimiento = recepcion.FechaRecepcion,
                    Cantidad = detalle.CantidadUnidades,
                    TotalMovimiento = lote.PrecioTotal,
                    IdUnidadMedida = idUnidadMedida,
                    IdProveedor = orden.IdProveedor,
                    IdUsuario = idUsuario,
                Observacion = $"Recepción OC #{orden.Id} - {producto.Nombre}",
                Estado = 1
            };

            await _movimientosProductosRepository.CreateAsync(movimiento);
        }

        private static OrdenCompraResponseDTO MapToResponse(OrdenCompraEntity e, string? nombreProveedor) => new()
        {
            Id = e.Id,
            IdProveedor = e.IdProveedor,
            NombreProveedor = nombreProveedor,
            FechaEmision = e.FechaEmision,
            FechaRecepcion = e.FechaRecepcion,
            Estado = e.Estado,
            NombreEstado = ObtenerNombreEstado(e.Estado),
            Subtotal = e.Subtotal,
            Descuento = e.Descuento,
            Impuesto = e.Impuesto,
            Total = e.Total,
            Observaciones = e.Observaciones
        };

        private static string? ObtenerNombreEstado(int estado)
        {
            return estado switch
            {
                1 => "Pendiente",
                2 => "Aprobada",
                3 => "Recibida",
                4 => "Cancelada",
                _ => null
            };
        }
    }
}
