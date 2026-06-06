using DistribuidoraLaVilla.Domain.DTOS;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(string entidad, string? idEntidad, string accion, string? detalle, Guid idUsuario);
        Task<List<AuditoriaDTO>> ObtenerAsync(string? entidad = null, DateTime? desde = null, DateTime? hasta = null, int? idUsuario = null, int limit = 100);
    }
}
