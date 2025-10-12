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
    public class CategoriasMateriaPrimaService(IGenericRepository<CategoriaMateriaPrimaEntity, int> categoriasMateriaPrima)
    {
        private readonly IGenericRepository<CategoriaMateriaPrimaEntity, int> _categoriasMateriaPrima = categoriasMateriaPrima;

        public async Task CrearCategoriaAsync(CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO)
        {
            CategoriaMateriaPrimaEntity categoriaMateriaPrimaEntity = new()
            {
                Nombre = categoriasMateriaPrimaDTO.Nombre,
                Descripcion = categoriasMateriaPrimaDTO.Descripcion,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                Estado = 1
            };

            await _categoriasMateriaPrima.CreateAsync(categoriaMateriaPrimaEntity);
        }

        public async Task<List<CategoriaMateriaPrimaEntity>> ObtenerCategoriasAsync()
        {
            return await _categoriasMateriaPrima.GetAllAsync();
        }

        public async Task ActualizarEstadoCategorias(MateriaPrimaActualizarEstadoDTO categoriasMateriaPrimaActualizarEstadoDTO)
        {
            var categoriaMateriaPrima = await _categoriasMateriaPrima.FindByIdAsync(categoriasMateriaPrimaActualizarEstadoDTO.Id);

            if (categoriaMateriaPrima != null)
            {
                categoriaMateriaPrima.Estado = categoriasMateriaPrimaActualizarEstadoDTO.EstadoNuevo;
                categoriaMateriaPrima.FechaActualizacion = DateTime.Now;
                await _categoriasMateriaPrima.UpdateAsync(categoriaMateriaPrima);
            }
            else
            {
                throw new Exception("La categoria de materia prima no existe");
            }
        }

        public async Task ActualizarCategorias(int id, CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO)
        {
            var categoriaMateriaPrima = await _categoriasMateriaPrima.FindByIdAsync(id);

            if (categoriaMateriaPrima != null)
            {
                categoriaMateriaPrima.Nombre = categoriasMateriaPrimaDTO.Nombre;
                categoriaMateriaPrima.Descripcion = categoriasMateriaPrimaDTO.Descripcion;
                categoriaMateriaPrima.FechaActualizacion = DateTime.Now;

                await _categoriasMateriaPrima.UpdateAsync(categoriaMateriaPrima);
            }
            else
            {
                throw new Exception("Error al actualizar las categorias");
            }
        }

        public async Task<CategoriaMateriaPrimaEntity> ObtenerPorIdCategoria(int idCategoria)
        {
            var resultado = await _categoriasMateriaPrima.FindByIdAsync(idCategoria);
            if (resultado != null)
            {
                return resultado;
            }
            else
            {
                throw new Exception("No se encontro la categoria de materia prima");
            }
        }
    }
}
