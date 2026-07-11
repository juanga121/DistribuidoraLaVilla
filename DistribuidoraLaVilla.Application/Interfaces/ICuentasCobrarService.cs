using DistribuidoraLaVilla.Domain.DTOS.CxC;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface ICuentasCobrarService
    {
        Task<List<CuentaCobrarDTO>> ObtenerPendientesAsync();
        Task<List<CuentaCobrarDTO>> ObtenerVencidasAsync();
        Task<List<CuentaCobrarDTO>> ObtenerPagadasAsync();
        Task<EstadoCuentaDTO?> ObtenerEstadoCuentaAsync(string idCliente);
        Task<PagoResponseDTO> RegistrarPagoAsync(RegistrarPagoDTO dto);
    }
}
