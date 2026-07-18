using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class CuentasPagarService : ICuentasPagarService
    {
        private readonly IGenericRepository<CuentasPagarEntity, int> _cxpRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly MovimientosFinancierosService _movimientosService;

        public CuentasPagarService(
            IGenericRepository<CuentasPagarEntity, int> cxpRepository,
            IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
            IAuditoriaService auditoriaService,
            MovimientosFinancierosService movimientosService)
        {
            _cxpRepository = cxpRepository;
            _proveedoresRepository = proveedoresRepository;
            _auditoriaService = auditoriaService;
            _movimientosService = movimientosService;
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

            if (estado.HasValue)
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
                entity.Estado = 2;

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

            if (entity.Estado != 1)
                throw new Exception("La cuenta por pagar no está pendiente de pago");

            if (dto.Monto <= 0)
                throw new Exception("El monto del pago debe ser mayor a cero");

            if (dto.Monto > entity.SaldoPendiente)
                throw new Exception("El pago excede el saldo pendiente");

            entity.SaldoPendiente -= dto.Monto;

            if (entity.SaldoPendiente == 0)
                entity.Estado = 2;

            entity.FechaActualizacion = DateTime.Now;
            entity.IdUsuario = idUsuario == Guid.Empty ? entity.IdUsuario : idUsuario;

            await _cxpRepository.UpdateAsync(entity);

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

            var proveedor = await _proveedoresRepository.FindByIdAsync(entity.IdProveedor);

            await _auditoriaService.RegistrarAsync(
                "CuentasPagar",
                entity.IdCuentaPagar.ToString(),
                "RegistrarPago",
                $"Pago de {dto.Monto} registrado. Saldo pendiente: {entity.SaldoPendiente}",
                idUsuario);

            return MapearADTO(entity, proveedor?.Nombre);
        }

        private static CuentasPagarDTO MapearADTO(CuentasPagarEntity entity, string? proveedorNombre)
        {
            return new CuentasPagarDTO
            {
                IdCuentaPagar = entity.IdCuentaPagar,
                IdProveedor = entity.IdProveedor,
                ProveedorNombre = proveedorNombre,
                IdOrdenCompra = entity.IdOrdenCompra,
                MontoTotal = entity.MontoTotal,
                SaldoPendiente = entity.SaldoPendiente,
                Descripcion = entity.Descripcion,
                FechaVencimiento = entity.FechaVencimiento,
                Estado = entity.Estado,
                EstadoDescripcion = ObtenerEstadoDescripcion(entity.Estado),
                FechaCreacion = entity.FechaCreacion,
                FechaActualizacion = entity.FechaActualizacion,
                IdUsuario = entity.IdUsuario
            };
        }

        private static string ObtenerEstadoDescripcion(int estado)
        {
            return estado switch
            {
                1 => "Pendiente",
                2 => "Pagada",
                3 => "Cancelada",
                _ => "Desconocido"
            };
        }
    }
}
