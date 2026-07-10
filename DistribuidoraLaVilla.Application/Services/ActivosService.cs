using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class ActivosService(IGenericRepository<ActivosEntity, int> activosRepository)
    {
        private readonly IGenericRepository<ActivosEntity, int> _activosRepository = activosRepository;

        public async Task CrearActivoAsync(ActivosDTO activosDTO, Guid idUsuario)
        {
            var activo = new ActivosEntity
            {
                Nombre = activosDTO.Nombre,
                Descripcion = activosDTO.Descripcion,
                Monto = activosDTO.Monto,
                Estado = activosDTO.Estado ?? 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
            };

            await _activosRepository.CreateAsync(activo);
        }

        public async Task<List<ActivosEntity>> ObtenerActivosAsync()
        {
            return await _activosRepository.GetAllAsync();
        }

        public async Task<ActivosEntity> ObtenerActivoPorIdAsync(int id)
        {
            var activo = await _activosRepository.FindByIdAsync(id);
            return activo ?? throw new Exception("El activo no existe");
        }

        public async Task ActualizarActivoAsync(int id, ActivosDTO activosDTO, Guid idUsuario)
        {
            var activo = await _activosRepository.FindByIdAsync(id);
            if (activo == null)
            {
                throw new Exception("El activo no existe");
            }

            activo.Nombre = activosDTO.Nombre;
            activo.Descripcion = activosDTO.Descripcion;
            activo.Monto = activosDTO.Monto;
            activo.Estado = activosDTO.Estado ?? activo.Estado;
            activo.FechaActualizacion = DateTime.Now;
            activo.IdUsuario = idUsuario == Guid.Empty ? activo.IdUsuario : idUsuario;

            await _activosRepository.UpdateAsync(activo);
        }

        public async Task EliminarActivoAsync(int id)
        {
            var existente = await _activosRepository.FindByIdAsync(id);
            if (existente == null)
            {
                throw new Exception("El activo no existe");
            }

            await _activosRepository.DeleteAsync(id);
        }
    }
}
