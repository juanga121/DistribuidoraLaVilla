using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class PatrimonioService(IGenericRepository<PatrimonioEntity, int> patrimonioRepository)
    {
        private readonly IGenericRepository<PatrimonioEntity, int> _patrimonioRepository = patrimonioRepository;

        public async Task CrearPatrimonioAsync(PatrimonioDTO patrimonioDTO, Guid idUsuario)
        {
            var patrimonio = new PatrimonioEntity
            {
                Nombre = patrimonioDTO.Nombre,
                Descripcion = patrimonioDTO.Descripcion,
                Monto = patrimonioDTO.Monto,
                Estado = patrimonioDTO.Estado ?? 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
            };

            await _patrimonioRepository.CreateAsync(patrimonio);
        }

        public async Task<List<PatrimonioEntity>> ObtenerPatrimoniosAsync()
        {
            return await _patrimonioRepository.GetAllAsync();
        }

        public async Task<PatrimonioEntity> ObtenerPatrimonioPorIdAsync(int id)
        {
            var patrimonio = await _patrimonioRepository.FindByIdAsync(id);
            return patrimonio ?? throw new Exception("El patrimonio no existe");
        }

        public async Task ActualizarPatrimonioAsync(int id, PatrimonioDTO patrimonioDTO, Guid idUsuario)
        {
            var patrimonio = await _patrimonioRepository.FindByIdAsync(id);
            if (patrimonio == null)
            {
                throw new Exception("El patrimonio no existe");
            }

            patrimonio.Nombre = patrimonioDTO.Nombre;
            patrimonio.Descripcion = patrimonioDTO.Descripcion;
            patrimonio.Monto = patrimonioDTO.Monto;
            patrimonio.Estado = patrimonioDTO.Estado ?? patrimonio.Estado;
            patrimonio.FechaActualizacion = DateTime.Now;
            patrimonio.IdUsuario = idUsuario == Guid.Empty ? patrimonio.IdUsuario : idUsuario;

            await _patrimonioRepository.UpdateAsync(patrimonio);
        }

        public async Task EliminarPatrimonioAsync(int id)
        {
            var existente = await _patrimonioRepository.FindByIdAsync(id);
            if (existente == null)
            {
                throw new Exception("El patrimonio no existe");
            }

            await _patrimonioRepository.DeleteAsync(id);
        }
    }
}
