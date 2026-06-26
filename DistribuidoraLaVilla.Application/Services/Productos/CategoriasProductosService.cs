using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
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
        private readonly IAuditoriaService _auditoriaService;

        public CategoriasProductosService(IGenericRepository<CategoriasProductosEntity, int> categoriasProductosRepository, IAuditoriaService auditoriaService)
        {
            _categoriasProductosRepository = categoriasProductosRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task CrearCategoriaAsync(CategoriasProductosDTO categoriasProductosDTO, Guid idUsuario)
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
            await RegistrarAuditoriaAsync("CategoriaProducto", categoriasProductos.Id.ToString(), "Crear", new { nombre = categoriasProductos.Nombre, descripcion = categoriasProductos.Descripcion }, idUsuario);
        }

        public async Task<List<CategoriasProductosEntity>> ObtenerCategoriasAsync()
        {
            return await _categoriasProductosRepository.GetAllAsync();
        }

        public async Task ActualizarEstadoCategorias(ActualizarEstadoTipoIntDTO actualizarEstadoDTO, Guid idUsuario)
        {
            var categoria = await _categoriasProductosRepository.FindByIdAsync(actualizarEstadoDTO.Id);

            if (categoria != null)
            {
                categoria.Estado = actualizarEstadoDTO.EstadoNuevo;
                categoria.FechaActualizacion = DateTime.Now;
                await _categoriasProductosRepository.UpdateAsync(categoria);
                await RegistrarAuditoriaAsync("CategoriaProducto", categoria.Id.ToString(), "CambioEstado", new { estadoNuevo = categoria.Estado }, idUsuario);
            }
            else
            {
                throw new Exception("La categoria de producto no existe");
            }
        }

        public async Task ActualizarCategorias(int id, CategoriasProductosDTO categoriasProductosDTO, Guid idUsuario)
        {
            var categoria = await _categoriasProductosRepository.FindByIdAsync(id);

            if (categoria != null)
            {
                categoria.Nombre = categoriasProductosDTO.Nombre;
                categoria.Descripcion = categoriasProductosDTO.Descripcion;
                categoria.FechaActualizacion = DateTime.Now;

                await _categoriasProductosRepository.UpdateAsync(categoria);
                await RegistrarAuditoriaAsync("CategoriaProducto", categoria.Id.ToString(), "Modificar", new { nombre = categoria.Nombre, descripcion = categoria.Descripcion }, idUsuario);
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

        public async Task EliminarCategoriaAsync(int idCategoria, Guid idUsuario)
        {
            var existente = await _categoriasProductosRepository.FindByIdAsync(idCategoria);
            if (existente != null)
            {
                await _categoriasProductosRepository.DeleteAsync(idCategoria);
                await RegistrarAuditoriaAsync("CategoriaProducto", idCategoria.ToString(), "Eliminar", new { nombre = existente.Nombre }, idUsuario);
            }
            else
            {
                throw new Exception("La categoria de producto no existe");
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
