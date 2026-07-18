using DistribuidoraLaVilla.Domain.DTOS;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface ICuentasPagarService
    {
        Task<CuentasPagarDTO> CrearAsync(CrearCxPDTO dto, Guid idUsuario);
        Task<List<CuentasPagarDTO>> ObtenerTodosAsync(int? estado = null);
        Task<CuentasPagarDTO> ObtenerPorIdAsync(int id);
        Task<CuentasPagarDTO> ActualizarAsync(int id, CrearCxPDTO dto, Guid idUsuario);
        Task EliminarAsync(int id);
        Task<CuentasPagarDTO> RegistrarPagoAsync(int id, RegistrarPagoCxPDTO dto, Guid idUsuario);
    }
}
