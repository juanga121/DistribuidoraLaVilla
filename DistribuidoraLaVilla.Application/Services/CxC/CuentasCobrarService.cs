using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.CxC;
using DistribuidoraLaVilla.Domain.DTOS.Caja;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services.CxC
{
    public class CuentasCobrarService : ICuentasCobrarService
    {
        private readonly IGenericRepository<CuentasCobrarEntity, int> _cxcRepo;
        private readonly IGenericRepository<PagoCuentaEntity, int> _pagoRepo;
        private readonly IGenericRepository<FacturaEntity, int> _facturaRepo;
        private readonly IGenericRepository<ClientesEntity, Guid> _clienteRepo;
        private readonly IGenericRepository<ReciboEntity, int> _reciboRepo;
        private readonly ICajaService _cajaService;
        private readonly IUnitOfWork _unitOfWork;

        public CuentasCobrarService(
            IGenericRepository<CuentasCobrarEntity, int> cxcRepo,
            IGenericRepository<PagoCuentaEntity, int> pagoRepo,
            IGenericRepository<FacturaEntity, int> facturaRepo,
            IGenericRepository<ClientesEntity, Guid> clienteRepo,
            IGenericRepository<ReciboEntity, int> reciboRepo,
            ICajaService cajaService,
            IUnitOfWork unitOfWork)
        {
            _cxcRepo = cxcRepo;
            _pagoRepo = pagoRepo;
            _facturaRepo = facturaRepo;
            _clienteRepo = clienteRepo;
            _reciboRepo = reciboRepo;
            _cajaService = cajaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CuentaCobrarDTO>> ObtenerPendientesAsync()
        {
            var pendientes = _cxcRepo.GetByFilter(c =>
                c.Estado == 1 && c.SaldoPendiente > 0);

            return await MapearCuentasConClienteAsync(pendientes);
        }

        public async Task<EstadoCuentaDTO?> ObtenerEstadoCuentaAsync(string idCliente)
        {
            if (!Guid.TryParse(idCliente, out var clienteGuid))
                return null;

            var cliente = await _clienteRepo.FindByIdAsync(clienteGuid);
            if (cliente == null)
                return null;

            var cxcList = _cxcRepo.GetByFilter(c =>
                c.IdCliente == clienteGuid &&
                c.SaldoPendiente > 0);

            var facturasPendientes = await MapearCuentasConClienteAsync(cxcList);

            var cxcIds = cxcList.Select(c => c.Id).ToList();
            var pagosRecientes = new List<PagoDTO>();

            foreach (var cxcId in cxcIds)
            {
                var pagos = _pagoRepo.GetByFilter(p => p.IdCxc == cxcId);
                pagosRecientes.AddRange(pagos.Select(MapToPagoDTO));
            }

            var limiteCredito = cliente.LimiteCredito ?? 0;
            var saldoPendienteTotal = cxcList.Sum(c => c.SaldoPendiente ?? 0);
            var creditoDisponible = limiteCredito - saldoPendienteTotal;
            if (creditoDisponible < 0) creditoDisponible = 0;

            return new EstadoCuentaDTO
            {
                IdCliente = cliente.IdCliente.ToString(),
                ClienteNombre = cliente.Nombre ?? string.Empty,
                ClienteDocumento = cliente.Documento ?? string.Empty,
                TipoPersona = cliente.TipoPersona,
                TipoPersonaDescripcion = cliente.TipoPersona == (int)TipoPersona.Juridica ? "Jurídica" : "Natural",
                LimiteCredito = limiteCredito,
                CreditoDisponible = creditoDisponible,
                FacturasPendientes = facturasPendientes,
                PagosRecientes = pagosRecientes.OrderByDescending(p => p.FechaPago).Take(20).ToList(),
                SaldoTotalPendiente = saldoPendienteTotal
            };
        }

        public async Task<List<CuentaCobrarDTO>> ObtenerVencidasAsync()
        {
            var ahora = DateTime.Now;
            var vencidas = _cxcRepo.GetByFilter(c =>
                c.Estado == 1 &&
                c.SaldoPendiente > 0 &&
                c.FechaVencimiento < ahora);

            return await MapearCuentasConClienteAsync(vencidas);
        }

        public async Task<List<CuentaCobrarDTO>> ObtenerPagadasAsync()
        {
            var pagadas = _cxcRepo.GetByFilter(c =>
                c.Estado == 2 || c.SaldoPendiente == 0);

            return await MapearCuentasConClienteAsync(pagadas);
        }

        public async Task<PagoResponseDTO> RegistrarPagoAsync(RegistrarPagoDTO dto)
        {
            if (dto.MontoPago <= 0)
            {
                return new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El monto del pago debe ser mayor a cero"
                };
            }

            var cxc = await _cxcRepo.FindByIdAsync(dto.IdCxc);
            if (cxc == null)
            {
                return new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"No se encontró la cuenta por cobrar con ID {dto.IdCxc}"
                };
            }

            if (cxc.Estado != 1)
            {
                return new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "La cuenta por cobrar no está pendiente"
                };
            }

            if (dto.MontoPago > (cxc.SaldoPendiente ?? 0))
            {
                return new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = "El monto del pago no puede superar el saldo pendiente"
                };
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var pago = new PagoCuentaEntity
                {
                    IdCxc = dto.IdCxc,
                    FechaPago = DateTime.Now,
                    MontoPago = dto.MontoPago,
                    MetodoPago = dto.MetodoPago,
                    Referencia = dto.Referencia,
                    IdUsuario = dto.IdUsuario,
                    Observacion = dto.Observacion
                };

                await _pagoRepo.CreateAsync(pago);

                cxc.SaldoPendiente = (cxc.SaldoPendiente ?? 0) - dto.MontoPago;

                if (cxc.SaldoPendiente == 0)
                {
                    cxc.Estado = 2;
                }

                await _cxcRepo.UpdateAsync(cxc);

                var cliente = cxc.IdCliente.HasValue
                    ? await _clienteRepo.FindByIdAsync(cxc.IdCliente.Value)
                    : null;

                var recibo = new ReciboEntity
                {
                    IdPago = pago.Id,
                    NumeroRecibo = $"RC-{pago.Id:D6}",
                    FechaEmision = DateTime.Now,
                    IdCliente = cxc.IdCliente,
                    ClienteNombre = cliente?.Nombre ?? string.Empty,
                    NumeroFactura = cxc.IdFactura.HasValue ? $"F{cxc.IdFactura:D6}" : null,
                    MontoPagado = dto.MontoPago,
                    MetodoPago = dto.MetodoPago,
                    IdUsuario = dto.IdUsuario
                };
                await _reciboRepo.CreateAsync(recibo);

                // Ingreso de caja dentro de la MISMA transacción (CA09/CA10): si no hay
                // caja abierta lanza error y se revierte todo (pago, saldo CxC, recibo),
                // en lugar de descartar el ingreso en silencio después del commit.
                await _cajaService.RegistrarIngresoPagoCxcTransaccionalAsync(
                    pago.Id,
                    recibo.Id,
                    dto.MontoPago,
                    dto.IdUsuario,
                    recibo.NumeroRecibo,
                    recibo.NumeroFactura,
                    dto.MetodoPago);

                await _unitOfWork.CommitAsync();

                return new PagoResponseDTO
                {
                    Exitoso = true,
                    IdPago = pago.Id,
                    ReciboId = recibo.Id,
                    NumeroRecibo = recibo.NumeroRecibo,
                    Mensaje = "Pago registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return new PagoResponseDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error al registrar el pago: {ex.Message}"
                };
            }
        }

        #region Private Methods

        private async Task<List<CuentaCobrarDTO>> MapearCuentasConClienteAsync(
            List<CuentasCobrarEntity> cuentas)
        {
            var ahora = DateTime.Now;
            var resultado = new List<CuentaCobrarDTO>();

            foreach (var cxc in cuentas)
            {
                string clienteNombre = string.Empty;
                if (cxc.IdCliente.HasValue)
                {
                    var cliente = await _clienteRepo.FindByIdAsync(cxc.IdCliente.Value);
                    if (cliente != null)
                        clienteNombre = cliente.Nombre ?? string.Empty;
                }

                var fechaVencimiento = cxc.FechaVencimiento ?? DateTime.Now;
                var diasVencidos = (ahora > fechaVencimiento)
                    ? (int)(ahora - fechaVencimiento).TotalDays
                    : 0;

                resultado.Add(new CuentaCobrarDTO
                {
                    Id = cxc.Id,
                    IdFactura = cxc.IdFactura ?? 0,
                    IdCliente = cxc.IdCliente?.ToString() ?? string.Empty,
                    ClienteNombre = clienteNombre,
                    FechaEmision = cxc.FechaEmision ?? DateTime.Now,
                    FechaVencimiento = fechaVencimiento,
                    MontoTotal = cxc.MontoTotal ?? 0,
                    SaldoPendiente = cxc.SaldoPendiente ?? 0,
                    Estado = cxc.Estado == 2 ? "Pagada" : "Pendiente",
                    DiasVencidos = diasVencidos
                });
            }

            return resultado;
        }

        private static PagoDTO MapToPagoDTO(PagoCuentaEntity pago)
        {
            return new PagoDTO
            {
                Id = pago.Id,
                IdCxc = pago.IdCxc ?? 0,
                FechaPago = pago.FechaPago ?? DateTime.Now,
                MontoPago = pago.MontoPago ?? 0,
                MetodoPago = pago.MetodoPago ?? 0,
                Referencia = pago.Referencia,
                Observacion = pago.Observacion
            };
        }

        #endregion
    }
}
