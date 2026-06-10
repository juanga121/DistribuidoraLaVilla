using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraLaVilla.Application.Services.Caja
{
    public class RecibosCajaService : IRecibosCajaService
    {
        private readonly IGenericRepository<ReciboEntity, int> _reciboRepo;

        public RecibosCajaService(IGenericRepository<ReciboEntity, int> reciboRepo)
        {
            _reciboRepo = reciboRepo;
        }

        public async Task<List<ReciboEntity>> GetAllAsync(DateTime? desde, DateTime? hasta, int? metodoPago, string? search)
        {
            var query = _reciboRepo.GetQueryable();

            if (desde.HasValue)
                query = query.Where(r => r.FechaEmision >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(r => r.FechaEmision <= hasta.Value);

            if (metodoPago.HasValue)
                query = query.Where(r => r.MetodoPago == metodoPago.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r =>
                    r.NumeroRecibo.Contains(search) ||
                    (r.ClienteNombre != null && r.ClienteNombre.Contains(search)));

            return await query
                .OrderByDescending(r => r.FechaEmision)
                .ToListAsync();
        }

        public async Task<ReciboEntity?> GetByIdAsync(int id)
        {
            return await _reciboRepo.FindByIdAsync(id);
        }
    }
}
