using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Caja;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraLaVilla.Application.Services.Caja
{
    public class CajaService : ICajaService
    {
        private readonly IGenericRepository<CajaAperturaEntity, int> _aperturaRepo;
        private readonly IGenericRepository<CajaMovimientoEntity, int> _movimientoRepo;
        private readonly IGenericRepository<UsuariosEntity, Guid> _usuarioRepo;
        private readonly IGenericRepository<FacturaEntity, int> _facturaRepo;
        private readonly IGenericRepository<PagoCuentaEntity, int> _pagoRepo;
        private readonly IGenericRepository<ReciboEntity, int> _reciboRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CajaService(
            IGenericRepository<CajaAperturaEntity, int> aperturaRepo,
            IGenericRepository<CajaMovimientoEntity, int> movimientoRepo,
            IGenericRepository<UsuariosEntity, Guid> usuarioRepo,
            IGenericRepository<FacturaEntity, int> facturaRepo,
            IGenericRepository<PagoCuentaEntity, int> pagoRepo,
            IGenericRepository<ReciboEntity, int> reciboRepo,
            IUnitOfWork unitOfWork)
        {
            _aperturaRepo = aperturaRepo;
            _movimientoRepo = movimientoRepo;
            _usuarioRepo = usuarioRepo;
            _facturaRepo = facturaRepo;
            _pagoRepo = pagoRepo;
            _reciboRepo = reciboRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<CajaAperturaDTO?> ObtenerCajaActivaAsync()
        {
            var caja = await GetCajaAbiertaAsync();
            if (caja == null)
                return null;

            return await MapCajaAperturaAsync(caja);
        }

        public async Task<CajaAperturaDTO> AbrirCajaAsync(AbrirCajaDTO dto)
        {
            if (dto.MontoInicial < 0)
                throw new InvalidOperationException("El monto inicial no puede ser negativo");

            var abierta = await GetCajaAbiertaAsync();
            if (abierta != null)
                throw new InvalidOperationException("Ya existe una caja abierta");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var entity = new CajaAperturaEntity
                {
                    IdUsuario = dto.IdUsuario,
                    FechaApertura = DateTime.Now,
                    MontoInicial = dto.MontoInicial,
                    Estado = (int)EstadoCajaEnum.Abierta
                };

                await _aperturaRepo.CreateAsync(entity);
                await _unitOfWork.CommitAsync();

                return await MapCajaAperturaAsync(entity);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CajaCierreDTO> CerrarCajaAsync(CerrarCajaDTO dto)
        {
            var caja = await GetCajaAbiertaAsync();
            if (caja == null)
                throw new InvalidOperationException("No hay una caja abierta");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var resumen = await GetResumenCajaAsync(caja.Id);
                var esperado = caja.MontoInicial + resumen.Ingresos - resumen.Egresos;

                caja.FechaCierre = DateTime.Now;
                caja.MontoFinal = dto.MontoFinal;
                caja.TotalIngresos = resumen.Ingresos;
                caja.TotalEgresos = resumen.Egresos;
                caja.Diferencia = dto.MontoFinal - esperado;
                caja.Estado = (int)EstadoCajaEnum.Cerrada;

                await _aperturaRepo.UpdateAsync(caja);
                await _unitOfWork.CommitAsync();

                var result = await MapCajaAperturaAsync(caja);
                return new CajaCierreDTO
                {
                    IdApertura = result.IdApertura,
                    IdUsuario = result.IdUsuario,
                    UsuarioNombre = result.UsuarioNombre,
                    FechaApertura = result.FechaApertura,
                    FechaCierre = result.FechaCierre,
                    MontoInicial = result.MontoInicial,
                    MontoFinal = result.MontoFinal,
                    TotalIngresos = result.TotalIngresos,
                    TotalEgresos = result.TotalEgresos,
                    SaldoActual = result.SaldoActual,
                    Diferencia = result.Diferencia,
                    Estado = result.Estado,
                    CantidadMovimientos = result.CantidadMovimientos,
                    Esperado = esperado
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CajaMovimientoDTO> RegistrarEgresoAsync(RegistrarEgresoDTO dto)
        {
            if (dto.Monto <= 0)
                throw new InvalidOperationException("El monto del egreso debe ser mayor a cero");

            if (string.IsNullOrWhiteSpace(dto.Concepto))
                throw new InvalidOperationException("El concepto del egreso es obligatorio");

            var caja = await GetCajaAbiertaAsync();
            if (caja == null)
                throw new InvalidOperationException("No hay una caja abierta");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var movimiento = new CajaMovimientoEntity
                {
                    IdApertura = caja.Id,
                    TipoMovimiento = (int)TipoMovimientoCajaEnum.Egreso,
                    Concepto = dto.Concepto.Trim(),
                    Monto = dto.Monto,
                    Fecha = DateTime.Now,
                    IdUsuario = dto.IdUsuario
                };

                await _movimientoRepo.CreateAsync(movimiento);
                await _unitOfWork.CommitAsync();

                return await MapMovimientoAsync(movimiento);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CajaMovimientoDTO?> RegistrarIngresoFacturaContadoAsync(int idFactura, decimal monto, Guid idUsuario, string? numeroFactura = null, int? metodoPago = null)
        {
            if (monto <= 0)
                return null;

            var caja = await GetCajaAbiertaAsync();
            if (caja == null)
                return null;

            var concepto = string.IsNullOrWhiteSpace(numeroFactura)
                ? $"Venta contado - Factura {idFactura:D6}"
                : $"Venta contado - Factura {numeroFactura}";

            var movimiento = new CajaMovimientoEntity
            {
                IdApertura = caja.Id,
                TipoMovimiento = (int)TipoMovimientoCajaEnum.Ingreso,
                IdFactura = idFactura,
                Concepto = concepto,
                Monto = monto,
                Fecha = DateTime.Now,
                IdUsuario = idUsuario,
                MetodoPago = metodoPago
            };

            await _movimientoRepo.CreateAsync(movimiento);
            return await MapMovimientoAsync(movimiento);
        }

        public async Task<CajaMovimientoDTO?> RegistrarIngresoPagoCxcAsync(int idPago, int? idRecibo, decimal monto, Guid idUsuario, string? numeroRecibo = null, string? numeroFactura = null, int? metodoPago = null)
        {
            if (monto <= 0)
                return null;

            var caja = await GetCajaAbiertaAsync();
            if (caja == null)
                return null;

            var pago = await _pagoRepo.FindByIdAsync(idPago);
            var recibo = idRecibo.HasValue ? await _reciboRepo.FindByIdAsync(idRecibo.Value) : null;

            var conceptoBase = !string.IsNullOrWhiteSpace(numeroRecibo)
                ? $"CxC - Recibo {numeroRecibo}"
                : $"CxC - Pago {idPago:D6}";

            if (!string.IsNullOrWhiteSpace(numeroFactura))
                conceptoBase += $" / Factura {numeroFactura}";

            var movimiento = new CajaMovimientoEntity
            {
                IdApertura = caja.Id,
                TipoMovimiento = (int)TipoMovimientoCajaEnum.Ingreso,
                IdPago = pago?.Id,
                IdRecibo = recibo?.Id,
                Concepto = conceptoBase,
                Monto = monto,
                Fecha = DateTime.Now,
                IdUsuario = idUsuario,
                MetodoPago = metodoPago
            };

            await _movimientoRepo.CreateAsync(movimiento);
            return await MapMovimientoAsync(movimiento);
        }

        public async Task<List<CajaMovimientoDTO>> ObtenerMovimientosAsync(DateTime? desde, DateTime? hasta, int? tipoMovimiento = null)
        {
            var query = _movimientoRepo.GetQueryable();

            if (desde.HasValue)
                query = query.Where(m => m.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(m => m.Fecha <= hasta.Value);

            if (tipoMovimiento.HasValue)
                query = query.Where(m => m.TipoMovimiento == tipoMovimiento.Value);

            var movimientos = await query
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            var resultado = new List<CajaMovimientoDTO>();
            foreach (var movimiento in movimientos)
            {
                resultado.Add(await MapMovimientoAsync(movimiento));
            }

            return resultado;
        }

        public Task<CajaReporteDTO> ObtenerReporteDiarioAsync()
        {
            var hoy = DateTime.Today;
            return ObtenerReporteAsync(hoy, hoy.AddDays(1).AddTicks(-1), "Diario");
        }

        public Task<CajaReporteDTO> ObtenerReporteSemanalAsync()
        {
            var hoy = DateTime.Today;
            var inicio = hoy.AddDays(-(int)hoy.DayOfWeek + (hoy.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var fin = inicio.AddDays(7).AddTicks(-1);
            return ObtenerReporteAsync(inicio, fin, "Semanal");
        }

        public Task<CajaReporteDTO> ObtenerReporteMensualAsync()
        {
            var inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fin = inicio.AddMonths(1).AddTicks(-1);
            return ObtenerReporteAsync(inicio, fin, "Mensual");
        }

        private async Task<CajaReporteDTO> ObtenerReporteAsync(DateTime desde, DateTime hasta, string periodo)
        {
            var movimientos = await ObtenerMovimientosAsync(desde, hasta);
            var ingresos = movimientos.Where(m => m.TipoMovimiento == (int)TipoMovimientoCajaEnum.Ingreso).Sum(m => m.Monto);
            var egresos = movimientos.Where(m => m.TipoMovimiento == (int)TipoMovimientoCajaEnum.Egreso).Sum(m => m.Monto);

            return new CajaReporteDTO
            {
                Periodo = periodo,
                Desde = desde,
                Hasta = hasta,
                Ingresos = ingresos,
                Egresos = egresos,
                Neto = ingresos - egresos,
                CantidadMovimientos = movimientos.Count,
                Movimientos = movimientos
            };
        }

        private async Task<CajaAperturaEntity?> GetCajaAbiertaAsync()
        {
            return await _aperturaRepo.GetQueryable()
                .Where(c => c.Estado == (int)EstadoCajaEnum.Abierta)
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefaultAsync();
        }

        private async Task<(decimal Ingresos, decimal Egresos, int Cantidad)> GetResumenCajaAsync(int idApertura)
        {
            var movimientos = await _movimientoRepo.GetQueryable()
                .Where(m => m.IdApertura == idApertura)
                .ToListAsync();

            var ingresos = movimientos.Where(m => m.TipoMovimiento == (int)TipoMovimientoCajaEnum.Ingreso).Sum(m => m.Monto);
            var egresos = movimientos.Where(m => m.TipoMovimiento == (int)TipoMovimientoCajaEnum.Egreso).Sum(m => m.Monto);

            return (ingresos, egresos, movimientos.Count);
        }

        private async Task<CajaAperturaDTO> MapCajaAperturaAsync(CajaAperturaEntity entity)
        {
            var resumen = await GetResumenCajaAsync(entity.Id);
            var usuarioNombre = await GetUsuarioNombreAsync(entity.IdUsuario);
            var saldoActual = entity.MontoInicial + resumen.Ingresos - resumen.Egresos;

            // Obtener movimientos para el desglose por método de pago
            var movimientos = await _movimientoRepo.GetQueryable()
                .Where(m => m.IdApertura == entity.Id)
                .ToListAsync();
            var desglose = GetDesgloseMetodosPago(movimientos);

            return new CajaAperturaDTO
            {
                IdApertura = entity.Id,
                IdUsuario = entity.IdUsuario,
                UsuarioNombre = usuarioNombre,
                FechaApertura = entity.FechaApertura,
                FechaCierre = entity.FechaCierre,
                MontoInicial = entity.MontoInicial,
                MontoFinal = entity.MontoFinal,
                TotalIngresos = resumen.Ingresos,
                TotalEgresos = resumen.Egresos,
                SaldoActual = saldoActual,
                Diferencia = entity.Diferencia,
                Estado = entity.Estado,
                CantidadMovimientos = resumen.Cantidad,
                DesgloseMetodosPago = desglose
            };
        }

        private async Task<CajaMovimientoDTO> MapMovimientoAsync(CajaMovimientoEntity entity)
        {
            var usuarioNombre = await GetUsuarioNombreAsync(entity.IdUsuario);
            var recibo = entity.IdRecibo.HasValue ? await _reciboRepo.FindByIdAsync(entity.IdRecibo.Value) : null;

            return new CajaMovimientoDTO
            {
                IdMovimiento = entity.Id,
                IdApertura = entity.IdApertura,
                TipoMovimiento = entity.TipoMovimiento,
                TipoMovimientoDescripcion = entity.TipoMovimiento == (int)TipoMovimientoCajaEnum.Ingreso ? "Ingreso" : "Egreso",
                IdFactura = entity.IdFactura,
                IdPago = entity.IdPago,
                IdRecibo = entity.IdRecibo,
                NumeroFactura = entity.IdFactura.HasValue ? $"F{entity.IdFactura.Value:D6}" : null,
                NumeroRecibo = recibo?.NumeroRecibo,
                Concepto = entity.Concepto,
                Monto = entity.Monto,
                Fecha = entity.Fecha,
                IdUsuario = entity.IdUsuario,
                UsuarioNombre = usuarioNombre,
                MetodoPago = entity.MetodoPago,
                MetodoPagoDescripcion = entity.MetodoPago switch
                {
                    1 => "Efectivo",
                    2 => "Transferencia",
                    3 => "Tarjeta",
                    _ => null
                }
            };
        }

        private async Task<string?> GetUsuarioNombreAsync(Guid idUsuario)
        {
            var usuario = await _usuarioRepo.FindByIdAsync(idUsuario);
            if (usuario == null)
                return null;

            return usuario.Nombre ?? usuario.Email;
        }

        private static Dictionary<string, decimal> GetDesgloseMetodosPago(List<CajaMovimientoEntity> movimientos)
        {
            return movimientos
                .Where(m => m.TipoMovimiento == (int)TipoMovimientoCajaEnum.Ingreso && m.MetodoPago.HasValue)
                .GroupBy(m => m.MetodoPago.Value)
                .ToDictionary(
                    g => g.Key switch
                    {
                        1 => "Efectivo",
                        2 => "Transferencia",
                        3 => "Tarjeta",
                        _ => "Otro"
                    },
                    g => g.Sum(m => m.Monto)
                );
        }
    }
}
