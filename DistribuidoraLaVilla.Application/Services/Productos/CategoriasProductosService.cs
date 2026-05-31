using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class CategoriasProductosService
    {
        private readonly IGenericRepository<CategoriasProductosEntity, int> _categoriasProductosRepository;

        public CategoriasProductosService(IGenericRepository<CategoriasProductosEntity, int> categoriasProductosRepository)
        {
            _categoriasProductosRepository = categoriasProductosRepository;
        }

        public async Task CrearCategoriaAsync(CategoriasProductosDTO categoriasProductosDTO)
        {
            CategoriasProductosEntity categoriasProductos = new()
            {
                Nombre = categoriasProductosDTO.Nombre,
                Descripcion = categoriasProductosDTO.Descripcion,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                Estado = 1
            };

            await _categoriasProductosRepository.CreateAsync(categoriasProductos);
        }

        public async Task<List<CategoriasProductosEntity>> ObtenerCategoriasAsync()
        {
            return await _categoriasProductosRepository.GetAllAsync();
        }

        public async Task ActualizarEstadoCategorias(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var categoria = await _categoriasProductosRepository.FindByIdAsync(actualizarEstadoDTO.Id);

            if (categoria != null)
            {
                categoria.Estado = actualizarEstadoDTO.EstadoNuevo;
                categoria.FechaActualizacion = DateTime.Now;
                await _categoriasProductosRepository.UpdateAsync(categoria);
            }
            else
            {
                throw new Exception("La categoria de producto no existe");
            }
        }

        public async Task ActualizarCategorias(int id, CategoriasProductosDTO categoriasProductosDTO)
        {
            var categoria = await _categoriasProductosRepository.FindByIdAsync(id);

            if (categoria != null)
            {
                categoria.Nombre = categoriasProductosDTO.Nombre;
                categoria.Descripcion = categoriasProductosDTO.Descripcion;
                categoria.FechaActualizacion = DateTime.Now;

                await _categoriasProductosRepository.UpdateAsync(categoria);
            }
        }

        public async Task<CategoriasProductosEntity> ObtenerPorIdCategoria(int idCategoria)
        {
            var resultado = await _categoriasProductosRepository.FindByIdAsync(idCategoria);
            if (resultado != null)
            {
                return resultado;
            }
            else
            {
                throw new Exception("No se encontro la categoria de producto");
            }
        }

        public async Task<List<CategoriasProductosEntity>> ObtenerCategoriasDisponibles()
        {
            var resultado = await _categoriasProductosRepository.GetAllAsync();
            return [.. resultado.Where(c => c.Estado == 1)];
        }

        public async Task EliminarCategoriaAsync(int idCategoria)
        {
            var existente = await _categoriasProductosRepository.FindByIdAsync(idCategoria);
            if (existente != null)
            {
                await _categoriasProductosRepository.DeleteAsync(idCategoria);
            }
            else
            {
                throw new Exception("La categoria de producto no existe");
            }
        }
    }
}
