using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services.Auditoria
{
    public class AuditoriaService(
        IGenericRepository<AuditoriaEntity, int> auditoriaRepository,
        IGenericRepository<UsuariosEntity, Guid> usuariosRepository) : IAuditoriaService
    {
        private readonly IGenericRepository<AuditoriaEntity, int> _auditoriaRepository = auditoriaRepository;
        private readonly IGenericRepository<UsuariosEntity, Guid> _usuariosRepository = usuariosRepository;

        public async Task RegistrarAsync(string entidad, string? idEntidad, string accion, string? detalle, Guid idUsuario)
        {
            var auditoria = new AuditoriaEntity
            {
                Entidad = entidad,
                IdEntidad = idEntidad,
                Accion = accion,
                Detalle = detalle,
                IdUsuario = idUsuario,
                Fecha = DateTime.UtcNow
            };

            await _auditoriaRepository.CreateAsync(auditoria);
        }

        public async Task<List<AuditoriaDTO>> ObtenerAsync(string? entidad = null, DateTime? desde = null, DateTime? hasta = null, int? idUsuario = null, int limit = 100)
        {
            var query = _auditoriaRepository.GetQueryable();

            if (!string.IsNullOrEmpty(entidad))
                query = query.Where(a => a.Entidad == entidad);

            if (desde.HasValue)
                query = query.Where(a => a.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(a => a.Fecha <= hasta.Value);

            query = query.OrderByDescending(a => a.Fecha).Take(limit);

            var registros = query.ToList();
            var usuarios = _usuariosRepository.GetAllAsync().Result;

            var result = registros.Select(a => new AuditoriaDTO
            {
                Id = a.Id,
                Entidad = a.Entidad,
                IdEntidad = a.IdEntidad,
                Accion = a.Accion,
                Detalle = a.Detalle,
                Usuario = usuarios.FirstOrDefault(u => u.Id == a.IdUsuario)?.Nombre ?? a.IdUsuario.ToString(),
                Fecha = a.Fecha
            }).ToList();

            return result;
        }
    }
}
