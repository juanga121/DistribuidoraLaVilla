using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class MateriaPrimaService(IGenericRepository<MateriaPrimaEntity, int> materiaPrima)
    {
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrima = materiaPrima;
        public async Task CrearMateriaPrimaAsync(MateriaPrimaDTO materiaPrimaDTO)
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

        public async Task ActualizarEstadoMateriaPrima(MateriaPrimaActualizarEstadoDTO materiaPrimaActualizarEstadoDTO)
        {
            var materiaPrima = await _materiaPrima.FindByIdAsync(materiaPrimaActualizarEstadoDTO.Id);
            if (materiaPrima != null)
            {
                materiaPrima.Estado = materiaPrimaActualizarEstadoDTO.EstadoNuevo;
                materiaPrima.FechaActualizacion = DateTime.Now;
                await _materiaPrima.UpdateAsync(materiaPrima);
            }
            else
            {
                throw new Exception("La materia prima no existe");
            }
        }

        public async Task ActualizarMateriaPrima(int idMateriPrima, MateriaPrimaDTO materiaPrimaDTO)
        {
            var materiaPrima = await _materiaPrima.FindByIdAsync(idMateriPrima);
            if (materiaPrima != null)
            {
                materiaPrima.Nombre = materiaPrimaDTO.Nombre;
                materiaPrima.IdCategoria = materiaPrimaDTO.IdCategoria;
                materiaPrima.FechaActualizacion = DateTime.Now;
                await _materiaPrima.UpdateAsync(materiaPrima);
            }
            else
            {
                throw new Exception("Error al actualizar la materia prima");
            }
        }
        public async Task<List<MateriaPrimaEntity>> ObtenerMateriaPrimaDisponible()
        {
            var materiaPrima = await _materiaPrima.GetAllAsync();
            var materiaPrimaDisponible = materiaPrima.Where(item => item.Estado == 1).ToList();
            return materiaPrimaDisponible;
        }

        // New: delete materia prima by id using generic repository
        public async Task EliminarMateriaPrimaAsync(int idMateriaPrima)
        {
            var existente = await _materiaPrima.FindByIdAsync(idMateriaPrima);
            if (existente != null)
            {
                await _materiaPrima.DeleteAsync(idMateriaPrima);
            }
            else
            {
                throw new Exception("La materia prima no existe");
            }
        }
    }
}
