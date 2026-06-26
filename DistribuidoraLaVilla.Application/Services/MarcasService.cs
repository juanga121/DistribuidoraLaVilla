using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services
{
    public class MarcasService(IGenericRepository<MarcasEntity, int> marcasRepository, IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearMarcaAsync(MarcasDTO marcasDTO, Guid idUsuario)
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
            await RegistrarAuditoriaAsync("Marca", marcasEntity.IdMarca.ToString(), "Crear", new { nombre = marcasEntity.Nombre, descripcion = marcasEntity.Descripcion }, idUsuario);
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

        public async Task ActualizarEstadoMarca(ActualizarEstadoTipoIntDTO actualizarEstadoDTO, Guid idUsuario)
        {
            var marca = await _marcasRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (marca != null)
            {
                marca.Estado = actualizarEstadoDTO.EstadoNuevo;
                marca.FechaActualizacion = DateTime.Now;
                await _marcasRepository.UpdateAsync(marca);
                await RegistrarAuditoriaAsync("Marca", marca.IdMarca.ToString(), "CambioEstado", new { estadoNuevo = marca.Estado }, idUsuario);
            }
            else
            {
                throw new Exception("La marca no existe");
            }
        }

        public async Task ActualizarMarca(int idMarca, MarcasDTO marcasDTO, Guid idUsuario)
        {
            var marca = await _marcasRepository.FindByIdAsync(idMarca);
            if (marca != null)
            {
                marca.Nombre = marcasDTO.Nombre;
                marca.Descripcion = marcasDTO.Descripcion;
                marca.FechaActualizacion = DateTime.Now;
                await _marcasRepository.UpdateAsync(marca);
                await RegistrarAuditoriaAsync("Marca", marca.IdMarca.ToString(), "Modificar", new { nombre = marca.Nombre, descripcion = marca.Descripcion }, idUsuario);
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

        public async Task EliminarMarcaAsync(int idMarca, Guid idUsuario)
        {
            var existente = await _marcasRepository.FindByIdAsync(idMarca);
            if (existente != null)
            {
                await _marcasRepository.DeleteAsync(idMarca);
                await RegistrarAuditoriaAsync("Marca", idMarca.ToString(), "Eliminar", new { nombre = existente.Nombre }, idUsuario);
            }
            else
            {
                throw new Exception("La marca no existe");
            }
        }

        private async Task RegistrarAuditoriaAsync(string entidad, string? idEntidad, string accion, object detalle, Guid idUsuario)
        {
            try
            {
                await _auditoriaService.RegistrarAsync(entidad, idEntidad, accion, JsonSerializer.Serialize(detalle), idUsuario);
            }
            catch { }
        }
    }
}
