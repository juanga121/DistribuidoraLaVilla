using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Facturacion;
using DistribuidoraLaVilla.Domain.DTOS.CxC;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services.Facturacion
{
    public class FacturaService
    {
        private readonly IInventarioService _inventarioService;
        private readonly IGenericRepository<FacturaEntity, int> _facturaRepository;
        private readonly IGenericRepository<DetalleFacturaEntity, int> _detalleRepository;
        private readonly IGenericRepository<ClientesEntity, Guid> _clienteRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productoRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;
        private readonly IGenericRepository<UsuariosEntity, Guid> _usuariosRepository;
        private readonly IGenericRepository<CuentasCobrarEntity, int> _cxcRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICuentasCobrarService _cuentasCobrarService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ICajaService _cajaService;

        public FacturaService(
            IInventarioService inventarioService,
            IGenericRepository<FacturaEntity, int> facturaRepository,
            IGenericRepository<DetalleFacturaEntity, int> detalleRepository,
            IGenericRepository<ClientesEntity, Guid> clienteRepository,
            IGenericRepository<ProductosEntity, int> productoRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
            IGenericRepository<UsuariosEntity, Guid> usuariosRepository,
            IGenericRepository<CuentasCobrarEntity, int> cxcRepository,
            IUnitOfWork unitOfWork,
            ICuentasCobrarService cuentasCobrarService,
            IAuditoriaService auditoriaService,
            ICajaService cajaService)
        {
            _inventarioService = inventarioService;
            _facturaRepository = facturaRepository;
            _detalleRepository = detalleRepository;
            _clienteRepository = clienteRepository;
            _productoRepository = productoRepository;
            _unidadMedidaRepository = unidadMedidaRepository;
            _usuariosRepository = usuariosRepository;
            _cxcRepository = cxcRepository;
            _unitOfWork = unitOfWork;
            _cuentasCobrarService = cuentasCobrarService;
            _auditoriaService = auditoriaService;
            _cajaService = cajaService;
        }

        public async Task<FacturaResponseDTO> CrearFacturaContadoAsync(CrearFacturaDTO solicitud)
        {
            return await CrearFacturaBaseAsync(
                solicitud.IdCliente,
                solicitud.IdUsuario,
                solicitud.FormaPago,
                solicitud.MetodoPago,
                solicitud.Detalles,
                TipoFacturaEnum.Contado,
                null);
        }

        public async Task<FacturaResponseDTO> CrearFacturaCreditoAsync(CrearFacturaCreditoDTO solicitud)
        {
            var cliente = await _clienteRepository.FindByIdAsync(solicitud.IdCliente);
            if (cliente == null)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"No se encontró el cliente con ID {solicitud.IdCliente}"
                };
            }

            if (cliente.LimiteCredito == null)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El cliente no tiene límite de crédito asignado"
                };
            }

            if (solicitud.Detalles == null || solicitud.Detalles.Count == 0)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "La factura debe contener al menos un detalle"
                };
            }

            decimal totalEstimado = 0;
            foreach (var detalle in solicitud.Detalles)
            {
                var producto = await _productoRepository.FindByIdAsync(detalle.IdProducto);
                if (producto == null)
                {
                    return new FacturaResponseDTO
                    {
                        Exitoso = false,
                        Mensaje = $"No se encontró el producto con ID {detalle.IdProducto}"
                    };
                }

                var errorPrecio = ValidarPrecioDetalle(detalle);
                if (errorPrecio != null)
                {
                    return new FacturaResponseDTO
                    {
                        Exitoso = false,
                        Mensaje = errorPrecio
                    };
                }

                totalEstimado += detalle.Cantidad * detalle.Precio;
            }

            var cxcCliente = _cxcRepository.GetByFilter(c =>
                c.IdCliente == solicitud.IdCliente &&
                c.Estado == 1);

            var saldoPendienteTotal = cxcCliente.Sum(c => c.SaldoPendiente ?? 0);
            var creditoDisponible = cliente.LimiteCredito.Value - saldoPendienteTotal;

            if (totalEstimado > creditoDisponible)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El monto excede el límite de crédito disponible"
                };
            }

            var diasCredito = solicitud.DiasCredito ?? cliente.DiasCredito ?? 30;

            return await CrearFacturaBaseAsync(
                solicitud.IdCliente,
                solicitud.IdUsuario,
                solicitud.FormaPago,
                solicitud.MetodoPago,
                solicitud.Detalles,
                TipoFacturaEnum.Credito,
                diasCredito);
        }

        public async Task<List<FacturaEntity>> ObtenerFacturasAsync()
        {
            return await _facturaRepository.GetAllAsync();
        }

        public async Task<FacturaEntity?> ObtenerFacturaPorIdAsync(int id)
        {
            return await _facturaRepository.FindByIdAsync(id);
        }

        public async Task<List<DetalleFacturaEntity>> ObtenerDetallesPorFacturaAsync(int idFactura)
        {
            return _detalleRepository.GetByFilter(d => d.IdFactura == idFactura).ToList();
        }

        public async Task<FacturaTicketDTO?> GenerarTicketAsync(int idFactura)
        {
            var factura = await _facturaRepository.FindByIdAsync(idFactura);
            if (factura == null) return null;

            ClientesEntity? cliente = null;
            if (factura.IdCliente.HasValue)
            {
                cliente = await _clienteRepository.FindByIdAsync(factura.IdCliente.Value);
            }

            return await GenerarTicketAsync(idFactura, cliente);
        }

        #region Ticketera / POS

        /// <summary>
        /// Facturación rápida POS (Ticketera).
        /// Crea una factura de contado permitiendo cliente opcional
        /// (se resuelve a "Consumidor Final" si no se envía).
        /// </summary>
        public async Task<TicketeraResultDTO> CrearTicketeraAsync(CrearTicketeraDTO dto, Guid idUsuario)
        {
            Guid idCliente;
            string nombreCliente;

            if (dto.IdCliente.HasValue)
            {
                idCliente = dto.IdCliente.Value;
                var cliente = await _clienteRepository.FindByIdAsync(idCliente)
                    ?? throw new InvalidOperationException($"No se encontró el cliente con ID {idCliente}");
                nombreCliente = cliente.Nombre ?? "Consumidor Final";
            }
            else
            {
                nombreCliente = "Consumidor Final";
                idCliente = await ObtenerOCrearConsumidorFinalAsync();
            }

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                throw new InvalidOperationException("La factura debe contener al menos un detalle");

            var detalles = dto.Detalles.Select(d => new CrearDetalleFacturaDTO
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Precio = d.Precio,
                IdUnidadMedida = d.IdUnidadMedida,
                EsVentaPorPeso = d.EsVentaPorPeso
            }).ToList();

            var resultado = await CrearFacturaBaseAsync(
                idCliente,
                idUsuario,
                dto.FormaPago,
                dto.FormaPago,  // metodoPago = formaPago para POS
                detalles,
                TipoFacturaEnum.Contado,
                null             // sin crédito
            );

            if (!resultado.Exitoso)
                throw new InvalidOperationException(resultado.Mensaje ?? "Error al crear la factura POS");

            try
            {
                var resumen = JsonSerializer.Serialize(new
                {
                    tipoFactura = "POS",
                    total = resultado.Total,
                    cliente = nombreCliente
                });
                await _auditoriaService.RegistrarAsync("Factura", resultado.IdFactura.ToString(), "CrearPOS", resumen, idUsuario);
            }
            catch { /* fire-and-forget */ }

            var ticket = resultado.Ticket;
            var facturaCreada = await _facturaRepository.FindByIdAsync(resultado.IdFactura);

            return new TicketeraResultDTO
            {
                IdFactura = resultado.IdFactura,
                Cliente = nombreCliente,
                Total = resultado.Total,
                Lineas = ticket?.Lineas ?? new List<LineaTicketDTO>(),
                Fecha = facturaCreada?.Fecha ?? DateTime.Now,
                FormaPago = dto.FormaPago switch
                {
                    1 => "Efectivo",
                    2 => "Transferencia",
                    3 => "Tarjeta",
                    _ => $"FormaPago {dto.FormaPago}"
                }
            };
        }

        /// <summary>
        /// Busca un cliente "Consumidor Final" por email fijo.
        /// Si no existe, lo crea con Documento="0" y Estado=1.
        /// </summary>
        private async Task<Guid> ObtenerOCrearConsumidorFinalAsync()
        {
            const string emailConsumidor = "consumidor-final@local";

            var existente = _clienteRepository
                .GetByFilter(c => c.Email == emailConsumidor)
                .FirstOrDefault();

            if (existente != null)
                return existente.IdCliente;

            var nuevo = new ClientesEntity
            {
                IdCliente = Guid.NewGuid(),
                Nombre = "Consumidor Final",
                Documento = "0",
                Email = emailConsumidor,
                Estado = 1,
                FechaCreacion = DateTime.Now
            };

            await _clienteRepository.CreateAsync(nuevo);
            return nuevo.IdCliente;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Valida que el precio del detalle sea mayor a 0.
        /// Aplica a todas las líneas (unidad y peso) en todos los flujos de facturación.
        /// </summary>
        private static string? ValidarPrecioDetalle(CrearDetalleFacturaDTO detalle)
            => detalle.Precio <= 0 ? "El precio debe ser mayor a 0" : null;

        /// <summary>
        /// Método base transaccional para crear cualquier tipo de factura.
        /// Envuelve en IUnitOfWork: crea factura, detalles, consume inventario,
        /// y si es crédito, crea la cuenta por cobrar.
        /// </summary>
        private async Task<FacturaResponseDTO> CrearFacturaBaseAsync(
            Guid idCliente,
            Guid idUsuario,
            int formaPago,
            int metodoPago,
            List<CrearDetalleFacturaDTO> detalles,
            TipoFacturaEnum tipoFactura,
            int? diasCredito)
        {
            var cliente = await _clienteRepository.FindByIdAsync(idCliente);
            if (cliente == null)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"No se encontró el cliente con ID {idCliente}"
                };
            }

            if (detalles == null || detalles.Count == 0)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "La factura debe contener al menos un detalle"
                };
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var detallesEntidad = new List<DetalleFacturaEntity>();
                decimal totalFactura = 0;

                foreach (var detalle in detalles)
                {
                    var producto = await _productoRepository.FindByIdAsync(detalle.IdProducto);
                    if (producto == null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return new FacturaResponseDTO
                        {
                            Exitoso = false,
                            Mensaje = $"No se encontró el producto con ID {detalle.IdProducto}"
                        };
                    }

                    if (detalle.Cantidad <= 0)
                    {
                        await _unitOfWork.RollbackAsync();
                        return new FacturaResponseDTO
                        {
                            Exitoso = false,
                            Mensaje = $"La cantidad debe ser mayor a 0 para el producto '{producto.Nombre}'"
                        };
                    }

                    var esVentaPorPeso = detalle.EsVentaPorPeso ?? producto.VentaPorPeso;

                    var errorPrecio = ValidarPrecioDetalle(detalle);
                    if (errorPrecio != null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return new FacturaResponseDTO
                        {
                            Exitoso = false,
                            Mensaje = errorPrecio
                        };
                    }

                    var consumos = await _inventarioService.ConsumirLotesProductoAsync(
                        detalle.IdProducto,
                        detalle.Cantidad,
                        detalle.IdUnidadMedida,
                        idUsuario,
                        idCliente,
                        $"Venta Factura {(tipoFactura == TipoFacturaEnum.Credito ? "Crédito" : "Contado")} - Cliente: {cliente.Nombre ?? idCliente.ToString()}",
                        esVentaPorPeso,
                        producto.PesoPorUnidad
                    );

                    // A FIFO sale can consume stock from multiple lots. Persist one
                    // DetalleFactura row per lot (each with its own IdLote, quantity and
                    // subtotal) so the invoice detail reflects the actual lot breakdown.
                    // The invoice total is the sum of the per-lot subtotals, unchanged.
                    foreach (var consumo in consumos)
                    {
                        detallesEntidad.Add(new DetalleFacturaEntity
                        {
                            IdProducto = detalle.IdProducto,
                            IdLote = consumo.IdLote,
                            Cantidad = consumo.CantidadConsumida,
                            IdUnidadMedida = detalle.IdUnidadMedida,
                            Precio = detalle.Precio,
                            Subtotal = consumo.CantidadConsumida * detalle.Precio,
                            EsVentaPorPeso = esVentaPorPeso,
                            PesoTotal = esVentaPorPeso ? consumo.CantidadConsumida : null,
                            PrecioKilo = esVentaPorPeso ? detalle.Precio : null,
                            PrecioOriginal = esVentaPorPeso ? producto.PrecioPorKilo : (decimal?)producto.PrecioUnitario
                        });

                        totalFactura += consumo.CantidadConsumida * detalle.Precio;
                    }
                }

                var factura = new FacturaEntity
                {
                    IdCliente = idCliente,
                    IdUsuario = idUsuario,
                    Fecha = DateTime.Now,
                    TipoFactura = (int)tipoFactura,
                    FormaPago = formaPago,
                    MetodoPago = metodoPago,
                    Total = totalFactura,
                    Estado = 1
                };

                await _facturaRepository.CreateAsync(factura);

                foreach (var detalleEntidad in detallesEntidad)
                {
                    detalleEntidad.IdFactura = factura.Id;
                    await _detalleRepository.CreateAsync(detalleEntidad);
                }

                if (tipoFactura == TipoFacturaEnum.Credito && diasCredito.HasValue)
                {
                    var cxc = new CuentasCobrarEntity
                    {
                        IdFactura = factura.Id,
                        IdCliente = idCliente,
                        FechaEmision = DateTime.Now,
                        FechaVencimiento = DateTime.Now.AddDays(diasCredito.Value),
                        MontoTotal = totalFactura,
                        SaldoPendiente = totalFactura,
                        Estado = 1
                    };

                    await _cxcRepository.CreateAsync(cxc);
                }

                if (tipoFactura == TipoFacturaEnum.Contado)
                {
                    // Caja ingreso INSIDE the same transaction as the invoice: if there is no
                    // open cash register the sale is blocked and everything rolls back (CA10),
                    // instead of silently dropping the income after commit.
                    await _cajaService.RegistrarIngresoFacturaContadoTransaccionalAsync(factura.Id, totalFactura, idUsuario, $"F{factura.Id:D6}", metodoPago);
                }

                await _unitOfWork.CommitAsync();

                try
                {
                    var tipo = tipoFactura == TipoFacturaEnum.Contado ? "Contado" : "Crédito";
                    var resumen = JsonSerializer.Serialize(new
                    {
                        tipoFactura = tipo,
                        total = totalFactura,
                        cliente = cliente.Nombre ?? idCliente.ToString(),
                        cantidadDetalles = detalles.Count
                    });
                    await _auditoriaService.RegistrarAsync("Factura", factura.Id.ToString(), "Crear", resumen, idUsuario);
                }
                catch { /* fire-and-forget */ }

                var ticket = await GenerarTicketAsync(factura.Id, cliente);

                return new FacturaResponseDTO
                {
                    Exitoso = true,
                    IdFactura = factura.Id,
                    Total = totalFactura,
                    Mensaje = "Factura creada exitosamente",
                    Ticket = ticket
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task<FacturaTicketDTO> GenerarTicketAsync(int idFactura, ClientesEntity? cliente)
        {
            var factura = await _facturaRepository.FindByIdAsync(idFactura)
                ?? throw new InvalidOperationException($"No se encontró la factura con ID {idFactura}");

            string? cajeroNombre = null;
            if (factura.IdUsuario.HasValue)
            {
                var usuario = await _usuariosRepository.FindByIdAsync(factura.IdUsuario.Value);
                cajeroNombre = usuario?.Nombre ?? usuario?.Email;
            }

            var detalles = _detalleRepository.GetByFilter(d => d.IdFactura == idFactura).ToList();

            var lineas = new List<LineaTicketDTO>();
            decimal subtotal = 0;

            foreach (var detalle in detalles)
            {
                ProductosEntity? producto = null;
                if (detalle.IdProducto.HasValue)
                {
                    producto = await _productoRepository.FindByIdAsync(detalle.IdProducto.Value);
                }

                UnidadMedidaEntity? unidad = null;
                if (detalle.IdUnidadMedida.HasValue)
                {
                    unidad = await _unidadMedidaRepository.FindByIdAsync(detalle.IdUnidadMedida.Value);
                }

                decimal importeLinea = (detalle.Subtotal ?? 0);
                subtotal += importeLinea;

                lineas.Add(new LineaTicketDTO
                {
                    ProductoNombre = producto?.Nombre ?? $"Producto ID {detalle.IdProducto}",
                    Cantidad = detalle.Cantidad ?? 0,
                    UnidadMedida = unidad?.Abreviatura ?? "",
                    PrecioUnitario = detalle.Precio ?? 0,
                    PesoTotal = detalle.PesoTotal,
                    PrecioKilo = detalle.PrecioKilo,
                    Subtotal = importeLinea
                });
            }

            return new FacturaTicketDTO
            {
                EmpresaNombre = null,
                EmpresaDireccion = null,
                EmpresaTelefono = null,
                EmpresaCuit = null,
                EmpresaEmail = null,
                IdFactura = factura.Id,
                NumeroFactura = factura.Id.ToString("D6"),
                Fecha = factura.Fecha?.ToString("dd/MM/yyyy HH:mm"),
                TipoFactura = factura.TipoFactura == 1 ? "Contado" : "Crédito",
                FormaPago = factura.FormaPago?.ToString(),
                MetodoPago = factura.MetodoPago?.ToString(),
                ClienteNombre = cliente?.Nombre,
                ClienteDocumento = cliente?.Documento,
                ClienteDireccion = cliente?.Direccion,
                ClienteTelefono = cliente?.Telefono,
                Lineas = lineas,
                Subtotal = subtotal,
                Descuento = 0,
                Total = factura.Total ?? subtotal,
                TotalEnLetras = null,
                CajeroNombre = cajeroNombre,
                MensajePie = "Gracias por su compra"
            };
        }

        #endregion
    }
}
