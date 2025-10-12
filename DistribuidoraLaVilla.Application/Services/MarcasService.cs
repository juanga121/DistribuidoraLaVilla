using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services
{
    public class MarcasService(IGenericRepository<MarcasEntity, int> marcasRepository)
    {
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;

        public async Task CrearMarcaAsync(MarcasDTO marcasDTO)
        {
            MarcasEntity marcasEntity = new()
            {
                Nombre = marcasDTO.Nombre,
                Descripcion = marcasDTO.Descripcion,
                Estado = 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
            await _marcasRepository.CreateAsync(marcasEntity);
        }

        public async Task<List<MarcasEntity>> ObtenerMarcasAsync()
        {
            return await _marcasRepository.GetAllAsync();
        }

        public async Task<MarcasEntity> ObtenerMarcaPorIdAsync(int id)
        {
            var marca = await _marcasRepository.FindByIdAsync(id);
            return marca ?? throw new Exception("La marca no existe");
        }

        public async Task ActualizarEstadoMarca(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var marca = await _marcasRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (marca != null)
            {
                marca.Estado = actualizarEstadoDTO.EstadoNuevo;
                marca.FechaActualizacion = DateTime.Now;
                await _marcasRepository.UpdateAsync(marca);
            }
            else
            {
                throw new Exception("La marca no existe");
            }
        }

        public async Task ActualizarMarca(int idMarca, MarcasDTO marcasDTO)
        {
            var marca = await _marcasRepository.FindByIdAsync(idMarca);
            if (marca != null)
            {
                marca.Nombre = marcasDTO.Nombre;
                marca.Descripcion = marcasDTO.Descripcion;
                marca.FechaActualizacion = DateTime.Now;
                await _marcasRepository.UpdateAsync(marca);
            }
            else
            {
                throw new Exception("La marca no existe");
            }
        }
        public async Task<List<MarcasEntity>> ObtenerMarcasDisponibles()
        {
            var marcas = await _marcasRepository.GetAllAsync();
            var marcasDisponibles = marcas.Where(item => item.Estado == 1).ToList();
            return marcasDisponibles;
        }
    }
}
