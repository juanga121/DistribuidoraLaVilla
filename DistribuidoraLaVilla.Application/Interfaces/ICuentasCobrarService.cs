using DistribuidoraLaVilla.Domain.DTOS.CxC;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface ICuentasCobrarService
    {
        Task<List<CuentaCobrarDTO>> ObtenerPendientesAsync();
        Task<EstadoCuentaDTO?> ObtenerEstadoCuentaAsync(string idCliente);
        Task<List<CuentaCobrarDTO>> ObtenerVencidasAsync();
        Task<PagoResponseDTO> RegistrarPagoAsync(RegistrarPagoDTO dto);
    }
}
