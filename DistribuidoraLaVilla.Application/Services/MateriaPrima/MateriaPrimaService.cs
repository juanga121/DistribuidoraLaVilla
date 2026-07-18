using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class MateriaPrimaService(IGenericRepository<MateriaPrimaEntity, int> materiaPrima, IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrima = materiaPrima;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearMateriaPrimaAsync(MateriaPrimaDTO materiaPrimaDTO, Guid idUsuario)
        {
            MateriaPrimaEntity materiaPrimaEntity = new()
            {
                Nombre = materiaPrimaDTO.Nombre,
                IdCategoria = materiaPrimaDTO.IdCategoria,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                Estado = 1
            };
            await _materiaPrima.CreateAsync(materiaPrimaEntity);
            await RegistrarAuditoriaAsync("MateriaPrima", materiaPrimaEntity.Id.ToString(), "Crear", new { nombre = materiaPrimaEntity.Nombre, idCategoria = materiaPrimaEntity.IdCategoria }, idUsuario);
        }

        public async Task<List<MateriaPrimaEntity>> ObtenerMateriaPrimaAsync()
        {
            return await _materiaPrima.GetAllAsync();
        }

        public async Task<MateriaPrimaEntity> ObtnerMateriPrimaPorId(int idMateriaPrima)
        {
            var result = await _materiaPrima.FindByIdAsync(idMateriaPrima) ?? throw new Exception("No se encontro la materia prima");
            return result;
        }

        public async Task ActualizarEstadoMateriaPrima(MateriaPrimaActualizarEstadoDTO materiaPrimaActualizarEstadoDTO, Guid idUsuario)
        {
            var materiaPrima = await _materiaPrima.FindByIdAsync(materiaPrimaActualizarEstadoDTO.Id);
            if (materiaPrima != null)
            {
                materiaPrima.Estado = materiaPrimaActualizarEstadoDTO.EstadoNuevo;
                materiaPrima.FechaActualizacion = DateTime.Now;
                await _materiaPrima.UpdateAsync(materiaPrima);
                await RegistrarAuditoriaAsync("MateriaPrima", materiaPrima.Id.ToString(), "CambioEstado", new { estadoNuevo = materiaPrima.Estado }, idUsuario);
            }
            else
            {
                throw new Exception("La materia prima no existe");
            }
        }

        public async Task ActualizarMateriaPrima(int idMateriPrima, MateriaPrimaDTO materiaPrimaDTO, Guid idUsuario)
        {
            var materiaPrima = await _materiaPrima.FindByIdAsync(idMateriPrima);
            if (materiaPrima != null)
            {
                materiaPrima.Nombre = materiaPrimaDTO.Nombre;
                materiaPrima.IdCategoria = materiaPrimaDTO.IdCategoria;
                materiaPrima.FechaActualizacion = DateTime.Now;
                await _materiaPrima.UpdateAsync(materiaPrima);
                await RegistrarAuditoriaAsync("MateriaPrima", materiaPrima.Id.ToString(), "Modificar", new { nombre = materiaPrima.Nombre, idCategoria = materiaPrima.IdCategoria }, idUsuario);
            }
            else
            {
                throw new Exception("La materia prima no existe");
            }
        }
        public async Task<List<MateriaPrimaEntity>> ObtenerMateriaPrimaDisponible()
        {
            var materiaPrima = await _materiaPrima.GetAllAsync();
            var materiaPrimaDisponible = materiaPrima.Where(item => item.Estado == 1).ToList();
            return materiaPrimaDisponible;
        }

        public async Task EliminarMateriaPrimaAsync(int idMateriaPrima, Guid idUsuario)
        {
            var existente = await _materiaPrima.FindByIdAsync(idMateriaPrima);
            if (existente != null)
            {
                await _materiaPrima.DeleteAsync(idMateriaPrima);
                await RegistrarAuditoriaAsync("MateriaPrima", idMateriaPrima.ToString(), "Eliminar", new { nombre = existente.Nombre }, idUsuario);
            }
            else
            {
                throw new Exception("La materia prima no existe");
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
