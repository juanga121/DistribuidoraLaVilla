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
            // ── Validaciones específicas de crédito ──

            // Validar cliente
            var cliente = await _clienteRepository.FindByIdAsync(solicitud.IdCliente);
            if (cliente == null)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"No se encontró el cliente con ID {solicitud.IdCliente}"
                };
            }

            // BR-01: Cliente sin LimiteCredito no puede comprar a crédito
            if (cliente.LimiteCredito == null)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El cliente no tiene límite de crédito asignado"
                };
            }

            // Validar detalles
            if (solicitud.Detalles == null || solicitud.Detalles.Count == 0)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "La factura debe contener al menos un detalle"
                };
            }

            // ── Validar productos y calcular total estimado para crédito ──
            // NOTA: Este es un cálculo estimado (Cantidad × Precio del DTO).
            // El total REAL se calcula en CrearFacturaBaseAsync usando la cantidad
            // realmente consumida del inventario (FIFO). Esta validación es un guard
            // contra el límite de crédito, no el monto final.
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
                totalEstimado += detalle.Cantidad * detalle.Precio;
            }

            // Calcular crédito disponible
            var cxcCliente = _cxcRepository.GetByFilter(c =>
                c.IdCliente == solicitud.IdCliente &&
                c.Estado == 1);

            var saldoPendienteTotal = cxcCliente.Sum(c => c.SaldoPendiente ?? 0);
            var creditoDisponible = cliente.LimiteCredito.Value - saldoPendienteTotal;

            // BR-02: Total no debe exceder límite disponible
            if (totalEstimado > creditoDisponible)
            {
                return new FacturaResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El monto excede el límite de crédito disponible"
                };
            }

            // BR-03: DiasCredito del DTO o del cliente por defecto
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
            // ── 1. Resolver cliente ──
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

            // ── 2. Validar detalles ──
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                throw new InvalidOperationException("La factura debe contener al menos un detalle");

            // ── 3. Map DetalleTicketeraDTO → CrearDetalleFacturaDTO ──
            var detalles = dto.Detalles.Select(d => new CrearDetalleFacturaDTO
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Precio = d.Precio,
                IdUnidadMedida = d.IdUnidadMedida,
                EsVentaPorPeso = d.EsVentaPorPeso
            }).ToList();

            // ── 4. Crear factura (reusa la lógica transaccional base) ──
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

            // ── 5. Mapear a TicketeraResultDTO ──
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
            // ── Validaciones comunes ──

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

            // ── Transacción ──
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // ── Procesar detalles y consumir inventario ──
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

                    // Validate quantity > 0
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

                    // BR-VP-11: Reject zero price for weight products
                    if (esVentaPorPeso && detalle.Precio <= 0)
                    {
                        await _unitOfWork.RollbackAsync();
                        return new FacturaResponseDTO
                        {
                            Exitoso = false,
                            Mensaje = $"El precio debe ser mayor a 0 para el producto por peso '{producto.Nombre}'"
                        };
                    }

                    // Consumir lotes usando FIFO
                    var consumos = await _inventarioService.ConsumirLotesProductoAsync(
                        detalle.IdProducto,
                        detalle.Cantidad,
                        detalle.IdUnidadMedida,
                        idUsuario,
                        $"Venta Factura {(tipoFactura == TipoFacturaEnum.Credito ? "Crédito" : "Contado")} - Cliente: {cliente.Nombre ?? idCliente.ToString()}",
                        esVentaPorPeso,
                        producto.PesoPorUnidad
                    );

                    decimal cantidadTotalConsumida = consumos.Sum(c => c.CantidadConsumida);
                    decimal subtotal = cantidadTotalConsumida * detalle.Precio;
                    int? idLote = consumos.FirstOrDefault()?.IdLote;

                    detallesEntidad.Add(new DetalleFacturaEntity
                    {
                        IdProducto = detalle.IdProducto,
                        IdLote = idLote,
                        Cantidad = cantidadTotalConsumida,
                        IdUnidadMedida = detalle.IdUnidadMedida,
                        Precio = detalle.Precio,
                        Subtotal = subtotal,
                        EsVentaPorPeso = esVentaPorPeso,
                        PesoTotal = esVentaPorPeso ? cantidadTotalConsumida : null,
                        PrecioKilo = esVentaPorPeso ? detalle.Precio : null
                    });

                    totalFactura += subtotal;
                }

                // ── Crear factura ──
                var factura = new FacturaEntity
                {
                    IdCliente = idCliente,
                    IdUsuario = idUsuario,
                    Fecha = DateTime.Now,
                    TipoFactura = (int)tipoFactura,
                    FormaPago = formaPago,
                    MetodoPago = metodoPago,
                    Total = totalFactura,
                    Estado = 1 // Activo
                };

                await _facturaRepository.CreateAsync(factura);

                // ── Crear detalles ──
                foreach (var detalleEntidad in detallesEntidad)
                {
                    detalleEntidad.IdFactura = factura.Id;
                    await _detalleRepository.CreateAsync(detalleEntidad);
                }

                // ── Si es crédito, crear CuentasCobrarEntity ──
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
                        Estado = 1 // Pendiente
                    };

                    await _cxcRepository.CreateAsync(cxc);
                }

                await _unitOfWork.CommitAsync();

                if (tipoFactura == TipoFacturaEnum.Contado)
                {
                    await _cajaService.RegistrarIngresoFacturaContadoAsync(factura.Id, totalFactura, idUsuario, $"F{factura.Id:D6}", metodoPago);
                }

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

                // ── Generar ticket ──
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

            // Buscar nombre del cajero/usuario
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
