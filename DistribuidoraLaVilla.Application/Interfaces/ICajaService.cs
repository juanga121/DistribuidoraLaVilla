using DistribuidoraLaVilla.Domain.DTOS.Caja;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface ICajaService
    {
        Task<CajaAperturaDTO?> ObtenerCajaActivaAsync();
        Task<CajaAperturaDTO> AbrirCajaAsync(AbrirCajaDTO dto);
        Task<CajaCierreDTO> CerrarCajaAsync(CerrarCajaDTO dto);
        Task<CajaMovimientoDTO> RegistrarEgresoAsync(RegistrarEgresoDTO dto);
        Task<CajaMovimientoDTO?> RegistrarIngresoFacturaContadoAsync(int idFactura, decimal monto, Guid idUsuario, string? numeroFactura = null, int? metodoPago = null);
        Task<CajaMovimientoDTO?> RegistrarIngresoPagoCxcAsync(int idPago, int? idRecibo, decimal monto, Guid idUsuario, string? numeroRecibo = null, string? numeroFactura = null, int? metodoPago = null);
        Task<List<CajaMovimientoDTO>> ObtenerMovimientosAsync(DateTime? desde, DateTime? hasta, int? tipoMovimiento = null);
        Task<CajaReporteDTO> ObtenerReporteDiarioAsync();
        Task<CajaReporteDTO> ObtenerReporteSemanalAsync();
        Task<CajaReporteDTO> ObtenerReporteMensualAsync();
    }
}
