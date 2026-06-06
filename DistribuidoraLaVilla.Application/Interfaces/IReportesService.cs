using DistribuidoraLaVilla.Domain.DTOS.Reportes;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface IReportesService
    {
        Task<DashboardDTO> GetDashboardAsync();
        Task<VentasReporteDTO> GetVentasAsync(DateTime desde, DateTime hasta, Guid? idCliente, int? idProducto);
        Task<CxcAgingReporteDTO> GetCxcAgingAsync(Guid? idCliente);
        Task<InventarioReporteDTO> GetInventarioAsync(int? idCategoria, int stockThreshold = 10);
        Task<MovimientosReporteDTO> GetMovimientosAsync(DateTime desde, DateTime hasta, int? idProducto, int? tipoMovimiento);
        Task<ClienteReporteDTO?> GetClienteAsync(Guid idCliente);
        Task<MovimientosDiariosDTO> GetMovimientosDiariosAsync();
    }
}
