using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.DTOS.Caja;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class CuentasPagarService : ICuentasPagarService
    {
        private readonly IGenericRepository<CuentasPagarEntity, int> _cxpRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository;
        private readonly IGenericRepository<PagoCxPEntity, int> _pagoRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly MovimientosFinancierosService _movimientosService;
        private readonly ICajaService _cajaService;
        private readonly IUnitOfWork _unitOfWork;

        public CuentasPagarService(
            IGenericRepository<CuentasPagarEntity, int> cxpRepository,
            IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
            IAuditoriaService auditoriaService,
            MovimientosFinancierosService movimientosService)
            : this(
                cxpRepository: cxpRepository,
                proveedoresRepository: proveedoresRepository,
                pagoRepository: null,
                auditoriaService: auditoriaService,
                movimientosService: movimientosService,
                cajaService: null,
                unitOfWork: null)
        {
        }

        public CuentasPagarService(
            IGenericRepository<CuentasPagarEntity, int> cxpRepository,
            IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
            IGenericRepository<PagoCxPEntity, int> pagoRepository,
            IAuditoriaService auditoriaService,
            MovimientosFinancierosService movimientosService,
            ICajaService cajaService,
            IUnitOfWork unitOfWork)
        {
            _cxpRepository = cxpRepository;
            _proveedoresRepository = proveedoresRepository;
            _pagoRepository = pagoRepository;
            _auditoriaService = auditoriaService;
            _movimientosService = movimientosService;
            _cajaService = cajaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CuentasPagarDTO> CrearAsync(CrearCxPDTO dto, Guid idUsuario)
        {
            var proveedor = await _proveedoresRepository.FindByIdAsync(dto.IdProveedor);
            if (proveedor == null)
                throw new Exception("El proveedor especificado no existe");

            var entity = new CuentasPagarEntity
            {
                IdProveedor = dto.IdProveedor,
                IdOrdenCompra = dto.IdOrdenCompra,
                MontoTotal = dto.MontoTotal,
                SaldoPendiente = dto.MontoTotal,
                Descripcion = dto.Descripcion,
                FechaVencimiento = dto.FechaVencimiento,
                Estado = 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
            };

            await _cxpRepository.CreateAsync(entity);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "CxP",
                SubTipo = "Creacion",
                Descripcion = $"Creación de cuenta por pagar: {entity.Descripcion}",
                Monto = entity.MontoTotal,
                Direccion = "Egreso",
                OrigenModulo = "CuentasPagar",
                ReferenciaId = entity.IdCuentaPagar,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, idUsuario);

            await _auditoriaService.RegistrarAsync(
                "CuentasPagar",
                entity.IdCuentaPagar.ToString(),
                "Crear",
                $"Cuenta por pagar creada para proveedor {dto.IdProveedor}, monto: {dto.MontoTotal}",
                idUsuario);

            return MapearADTO(entity, proveedor.Nombre);
        }

        public async Task<List<CuentasPagarDTO>> ObtenerTodosAsync(int? estado = null)
        {
            List<CuentasPagarEntity> entities;

            if (estado == (int)EstadoCuentaPagar.Vencida)
            {
                var ahora = DateTime.Now;
                entities = _cxpRepository.GetByFilter(c =>
                    c.Estado == (int)EstadoCuentaPagar.Pendiente &&
                    c.SaldoPendiente > 0 &&
                    c.FechaVencimiento < ahora);
            }
            else if (estado == (int)EstadoCuentaPagar.ParcialmentePagada)
            {
                entities = _cxpRepository.GetByFilter(c =>
                    c.Estado == (int)EstadoCuentaPagar.Pendiente &&
                    c.SaldoPendiente > 0 &&
                    c.SaldoPendiente < c.MontoTotal);
            }
            else if (estado.HasValue)
            {
                entities = _cxpRepository.GetByFilter(c => c.Estado == estado.Value);
            }
            else
            {
                entities = await _cxpRepository.GetAllAsync();
            }

            var dtos = new List<CuentasPagarDTO>();
            foreach (var entity in entities)
            {
                var proveedor = await _proveedoresRepository.FindByIdAsync(entity.IdProveedor);
                dtos.Add(MapearADTO(entity, proveedor?.Nombre));
            }

            return dtos;
        }

        public async Task<CuentasPagarDTO> ObtenerPorIdAsync(int id)
        {
            var entity = await _cxpRepository.FindByIdAsync(id);
            if (entity == null)
                throw new Exception("La cuenta por pagar no existe");

            var proveedor = await _proveedoresRepository.FindByIdAsync(entity.IdProveedor);
            return MapearADTO(entity, proveedor?.Nombre);
        }

        public async Task<CuentasPagarDTO> ActualizarAsync(int id, CrearCxPDTO dto, Guid idUsuario)
        {
            var entity = await _cxpRepository.FindByIdAsync(id);
            if (entity == null)
                throw new Exception("La cuenta por pagar no existe");

            var proveedor = await _proveedoresRepository.FindByIdAsync(dto.IdProveedor);
            if (proveedor == null)
                throw new Exception("El proveedor especificado no existe");

            entity.IdProveedor = dto.IdProveedor;
            entity.IdOrdenCompra = dto.IdOrdenCompra;

            decimal montoPagado = entity.MontoTotal - entity.SaldoPendiente;
            entity.MontoTotal = dto.MontoTotal;
            entity.SaldoPendiente = dto.MontoTotal - montoPagado;

            if (entity.SaldoPendiente < 0)
                throw new Exception("El monto total no puede ser menor al monto ya pagado");

            if (entity.SaldoPendiente == 0)
                entity.Estado = (int)EstadoCuentaPagar.Pagada;

            entity.Descripcion = dto.Descripcion;
            entity.FechaVencimiento = dto.FechaVencimiento;
            entity.FechaActualizacion = DateTime.Now;
            entity.IdUsuario = idUsuario == Guid.Empty ? entity.IdUsuario : idUsuario;

            await _cxpRepository.UpdateAsync(entity);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "CxP",
                SubTipo = "Ajuste",
                Descripcion = $"Ajuste de cuenta por pagar #{id}",
                Monto = entity.MontoTotal,
                Direccion = "Neutro",
                OrigenModulo = "CuentasPagar",
                ReferenciaId = entity.IdCuentaPagar,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, idUsuario);

            await _auditoriaService.RegistrarAsync(
                "CuentasPagar",
                entity.IdCuentaPagar.ToString(),
                "Actualizar",
                $"Cuenta por pagar {id} actualizada",
                idUsuario);

            return MapearADTO(entity, proveedor.Nombre);
        }

        public async Task EliminarAsync(int id)
        {
            var entity = await _cxpRepository.FindByIdAsync(id);
            if (entity == null)
                throw new Exception("La cuenta por pagar no existe");

            entity.Estado = 3;
            entity.FechaActualizacion = DateTime.Now;

            await _cxpRepository.UpdateAsync(entity);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "CxP",
                SubTipo = "Baja",
                Descripcion = $"Baja de cuenta por pagar #{id}",
                Monto = entity.MontoTotal,
                Direccion = "Ingreso",
                OrigenModulo = "CuentasPagar",
                ReferenciaId = entity.IdCuentaPagar,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, entity.IdUsuario ?? Guid.Empty);

            await _auditoriaService.RegistrarAsync(
                "CuentasPagar",
                entity.IdCuentaPagar.ToString(),
                "Eliminar",
                $"Cuenta por pagar {id} cancelada",
                entity.IdUsuario ?? Guid.Empty);
        }

        public async Task<CuentasPagarDTO> RegistrarPagoAsync(int id, RegistrarPagoCxPDTO dto, Guid idUsuario)
        {
            var entity = await _cxpRepository.FindByIdAsync(id);
            if (entity == null)
                throw new Exception("La cuenta por pagar no existe");

            if (entity.Estado != (int)EstadoCuentaPagar.Pendiente)
                throw new Exception("La cuenta por pagar no está pendiente de pago");

            if (dto.Monto <= 0)
                throw new Exception("El monto del pago debe ser mayor a cero");

            if (dto.Monto > entity.SaldoPendiente)
                throw new Exception("El pago excede el saldo pendiente");

            if (_pagoRepository == null || _cajaService == null || _unitOfWork == null)
            {
                throw new InvalidOperationException(
                    "El registro de pago requiere caja, transacción y repositorio de pagos (infraestructura completa)");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                entity.SaldoPendiente -= dto.Monto;

                if (entity.SaldoPendiente == 0)
                    entity.Estado = (int)EstadoCuentaPagar.Pagada;

                entity.FechaActualizacion = DateTime.Now;
                entity.IdUsuario = idUsuario == Guid.Empty ? entity.IdUsuario : idUsuario;

                await _cxpRepository.UpdateAsync(entity);

                var pago = new PagoCxPEntity
                {
                    IdCuentaPagar = entity.IdCuentaPagar,
                    Monto = dto.Monto,
                    FechaPago = DateTime.Now,
                    FechaCreacion = DateTime.Now,
                    IdUsuario = idUsuario == Guid.Empty ? null : idUsuario,
                    MetodoPago = dto.MetodoPago,
                    Observacion = dto.Observacion
                };
                await _pagoRepository.CreateAsync(pago);

                await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
                {
                    TipoMovimiento = "CxP",
                    SubTipo = "Pago",
                    Descripcion = $"Pago de cuenta por pagar #{id}",
                    Monto = dto.Monto,
                    Direccion = "Egreso",
                    OrigenModulo = "CuentasPagar",
                    ReferenciaId = entity.IdCuentaPagar,
                    FechaMovimiento = DateTime.Now,
                    Estado = 1
                }, idUsuario);

                // Egreso de caja dentro de la MISMA transacción. Requiere caja abierta:
                // si no hay, lanza error y se revierte todo (saldo, historial, movimiento).
                await _cajaService.RegistrarEgresoTransaccionalAsync(new RegistrarEgresoDTO
                {
                    IdUsuario = idUsuario,
                    Monto = dto.Monto,
                    Concepto = $"Pago de cuenta por pagar #{entity.IdCuentaPagar}",
                    MetodoPago = dto.MetodoPago
                });

                await _auditoriaService.RegistrarAsync(
                    "CuentasPagar",
                    entity.IdCuentaPagar.ToString(),
                    "RegistrarPago",
                    $"Pago de {dto.Monto} registrado. Saldo pendiente: {entity.SaldoPendiente}",
                    idUsuario);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var proveedor = await _proveedoresRepository.FindByIdAsync(entity.IdProveedor);
            return MapearADTO(entity, proveedor?.Nombre);
        }

        public async Task<List<PagoCxPDTO>> ObtenerPagosAsync(int idCuentaPagar)
        {
            var cuenta = await _cxpRepository.FindByIdAsync(idCuentaPagar);
            if (cuenta == null)
                throw new Exception("La cuenta por pagar no existe");

            var pagos = _pagoRepository?.GetByFilter(p => p.IdCuentaPagar == idCuentaPagar)
                ?? new List<PagoCxPEntity>();

            return pagos
                .OrderByDescending(p => p.FechaPago)
                .Select(MapToPagoDTO)
                .ToList();
        }

        private static CuentasPagarDTO MapearADTO(CuentasPagarEntity entity, string? proveedorNombre)
        {
            var estadoEfectivo = ObtenerEstadoEfectivo(entity);
            var ahora = DateTime.Now;
            var fechaVencimiento = entity.FechaVencimiento ?? DateTime.Now;
            var diasVencidos = (ahora > fechaVencimiento)
                ? (int)(ahora - fechaVencimiento).TotalDays
                : 0;

            return new CuentasPagarDTO
            {
                IdCuentaPagar = entity.IdCuentaPagar,
                IdProveedor = entity.IdProveedor,
                ProveedorNombre = proveedorNombre,
                IdOrdenCompra = entity.IdOrdenCompra,
                MontoTotal = entity.MontoTotal,
                SaldoPendiente = entity.SaldoPendiente,
                MontoPagado = entity.MontoTotal - entity.SaldoPendiente,
                Descripcion = entity.Descripcion,
                FechaVencimiento = entity.FechaVencimiento,
                Estado = estadoEfectivo,
                EstadoDescripcion = ObtenerEstadoDescripcion(estadoEfectivo),
                DiasVencidos = diasVencidos,
                FechaCreacion = entity.FechaCreacion,
                FechaActualizacion = entity.FechaActualizacion,
                IdUsuario = entity.IdUsuario
            };
        }

        private static int ObtenerEstadoEfectivo(CuentasPagarEntity entity)
        {
            if (entity.Estado == (int)EstadoCuentaPagar.Cancelada)
                return (int)EstadoCuentaPagar.Cancelada;

            if (entity.Estado == (int)EstadoCuentaPagar.Pagada || entity.SaldoPendiente <= 0)
                return (int)EstadoCuentaPagar.Pagada;

            if (entity.FechaVencimiento.HasValue && entity.FechaVencimiento.Value < DateTime.Now)
                return (int)EstadoCuentaPagar.Vencida;

            if (entity.MontoTotal - entity.SaldoPendiente > 0)
                return (int)EstadoCuentaPagar.ParcialmentePagada;

            return (int)EstadoCuentaPagar.Pendiente;
        }

        private static string ObtenerEstadoDescripcion(int estado)
        {
            return estado switch
            {
                (int)EstadoCuentaPagar.Pendiente => "Pendiente",
                (int)EstadoCuentaPagar.Pagada => "Pagada",
                (int)EstadoCuentaPagar.Cancelada => "Cancelada",
                (int)EstadoCuentaPagar.ParcialmentePagada => "Parcialmente Pagada",
                (int)EstadoCuentaPagar.Vencida => "Vencida",
                _ => "Desconocido"
            };
        }

        private static PagoCxPDTO MapToPagoDTO(PagoCxPEntity pago)
        {
            return new PagoCxPDTO
            {
                IdPago = pago.IdPago,
                IdCuentaPagar = pago.IdCuentaPagar,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                IdUsuario = pago.IdUsuario,
                FechaCreacion = pago.FechaCreacion,
                MetodoPago = pago.MetodoPago,
                MetodoPagoDescripcion = pago.MetodoPago switch
                {
                    1 => "Efectivo",
                    2 => "Transferencia",
                    3 => "Tarjeta",
                    _ => null
                },
                Observacion = pago.Observacion
            };
        }
    }
}