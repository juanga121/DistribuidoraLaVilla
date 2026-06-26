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
    public class CategoriasMateriaPrimaService
    {
        private readonly IGenericRepository<CategoriaMateriaPrimaEntity, int> _categoriasMateriaPrima;
        private readonly IAuditoriaService _auditoriaService;

        public CategoriasMateriaPrimaService(IGenericRepository<CategoriaMateriaPrimaEntity, int> categoriasMateriaPrima, IAuditoriaService auditoriaService)
        {
            _categoriasMateriaPrima = categoriasMateriaPrima;
            _auditoriaService = auditoriaService;
        }

        public async Task CrearCategoriaAsync(CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO, Guid idUsuario)
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
            await RegistrarAuditoriaAsync("CategoriaMateriaPrima", categoriaMateriaPrimaEntity.Id.ToString(), "Crear", new { nombre = categoriaMateriaPrimaEntity.Nombre, descripcion = categoriaMateriaPrimaEntity.Descripcion }, idUsuario);
        }

        public async Task<List<CategoriaMateriaPrimaEntity>> ObtenerCategoriasAsync()
        {
            return await _categoriasMateriaPrima.GetAllAsync();
        }

        public async Task ActualizarEstadoCategorias(MateriaPrimaActualizarEstadoDTO categoriasMateriaPrimaActualizarEstadoDTO, Guid idUsuario)
        {
            var categoriaMateriaPrima = await _categoriasMateriaPrima.FindByIdAsync(categoriasMateriaPrimaActualizarEstadoDTO.Id);

            if (categoriaMateriaPrima != null)
            {
                categoriaMateriaPrima.Estado = categoriasMateriaPrimaActualizarEstadoDTO.EstadoNuevo;
                categoriaMateriaPrima.FechaActualizacion = DateTime.Now;
                await _categoriasMateriaPrima.UpdateAsync(categoriaMateriaPrima);
                await RegistrarAuditoriaAsync("CategoriaMateriaPrima", categoriaMateriaPrima.Id.ToString(), "CambioEstado", new { estadoNuevo = categoriaMateriaPrima.Estado }, idUsuario);
            }
            else
            {
                throw new Exception("La categoria de materia prima no existe");
            }
        }

        public async Task ActualizarCategorias(int id, CategoriasMateriaPrimaDTO categoriasMateriaPrimaDTO, Guid idUsuario)
        {
            var categoriaMateriaPrima = await _categoriasMateriaPrima.FindByIdAsync(id);

            if (categoriaMateriaPrima != null)
            {
                categoriaMateriaPrima.Nombre = categoriasMateriaPrimaDTO.Nombre;
                categoriaMateriaPrima.Descripcion = categoriasMateriaPrimaDTO.Descripcion;
                categoriaMateriaPrima.FechaActualizacion = DateTime.Now;

                await _categoriasMateriaPrima.UpdateAsync(categoriaMateriaPrima);
                await RegistrarAuditoriaAsync("CategoriaMateriaPrima", categoriaMateriaPrima.Id.ToString(), "Modificar", new { nombre = categoriaMateriaPrima.Nombre, descripcion = categoriaMateriaPrima.Descripcion }, idUsuario);
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

        public async Task<List<CategoriaMateriaPrimaEntity>> ObtenerCategoriasDisponibles()
        {
            var resultado = await _categoriasMateriaPrima.GetAllAsync();
            return [.. resultado.Where(c => c.Estado == 1)];
        }

        // New: delete category by id using generic repository
        public async Task EliminarCategoriaAsync(int idCategoria, Guid idUsuario)
        {
            var existente = await _categoriasMateriaPrima.FindByIdAsync(idCategoria);
            if (existente != null)
            {
                await _categoriasMateriaPrima.DeleteAsync(idCategoria);
                await RegistrarAuditoriaAsync("CategoriaMateriaPrima", idCategoria.ToString(), "Eliminar", new { nombre = existente.Nombre }, idUsuario);
            }
            else
            {
                throw new Exception("La categoria de materia prima no existe");
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
