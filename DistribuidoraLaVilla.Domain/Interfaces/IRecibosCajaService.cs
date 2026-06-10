using DistribuidoraLaVilla.Domain.Entities.Caja;

namespace DistribuidoraLaVilla.Domain.Interfaces
{
    public interface IRecibosCajaService
    {
        Task<List<ReciboEntity>> GetAllAsync(DateTime? desde, DateTime? hasta, int? metodoPago, string? search);
        Task<ReciboEntity?> GetByIdAsync(int id);
    }
}
