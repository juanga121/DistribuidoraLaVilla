using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class RecetaProductoService
    {
        private readonly IGenericRepository<RecetaProductoEntity, int> _recetaRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;
        private readonly RecetaProductoDTOValidator _validator;

        public RecetaProductoService(
            IGenericRepository<RecetaProductoEntity, int> recetaRepository,
            IGenericRepository<ProductosEntity, int> productosRepository,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository)
        {
            _recetaRepository = recetaRepository;
            _productosRepository = productosRepository;
            _materiaPrimaRepository = materiaPrimaRepository;
            _unidadMedidaRepository = unidadMedidaRepository;
            _validator = new RecetaProductoDTOValidator();
        }

        /// <summary>
        /// Crea una nueva receta de producto
        /// </summary>
        public async Task<RecetaProductoResponseDTO> CrearRecetaAsync(RecetaProductoDTO dto)
        {
            // 1. Validar con FluentValidation
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            // 2. Verificar que el producto exista
            var producto = await _productosRepository.FindByIdAsync(dto.IdProducto);
            if (producto == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {dto.IdProducto}");
            }

            // 3. Verificar que la materia prima exista
            var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(dto.IdMateriaPrima);
            if (materiaPrima == null)
            {
                throw new InvalidOperationException($"No se encontró la materia prima con ID {dto.IdMateriaPrima}");
            }

            // 4. Verificar que la unidad de medida exista
            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(dto.IdUnidadMedida);
            if (unidadMedida == null)
            {
                throw new InvalidOperationException($"No se encontró la unidad de medida con ID {dto.IdUnidadMedida}");
            }

            // 5. Validar que no exista una receta con la misma combinación producto-materia prima
            var recetaExistente = _recetaRepository.GetByFilter(r =>
                r.IdProducto == dto.IdProducto &&
                r.IdMateriaPrima == dto.IdMateriaPrima &&
                r.Estado == 1
            );

            if (recetaExistente.Any())
            {
                throw new InvalidOperationException(
                    $"Ya existe una receta activa para el producto '{producto.Nombre}' con la materia prima '{materiaPrima.Nombre}'. " +
                    "No se permiten materias primas repetidas en la misma receta."
                );
            }

            // 6. Crear la entidad
            var receta = new RecetaProductoEntity
            {
                IdProducto = dto.IdProducto,
                IdMateriaPrima = dto.IdMateriaPrima,
                CantidadRequerida = dto.CantidadRequerida,
                IdUnidadMedida = dto.IdUnidadMedida,
                Estado = 1
            };

            // 7. Guardar en repositorio
            await _recetaRepository.CreateAsync(receta);

            // 8. Retornar respuesta completa
            return new RecetaProductoResponseDTO
            {
                IdReceta = receta.Id,
                IdProducto = receta.IdProducto,
                NombreProducto = producto.Nombre,
                IdMateriaPrima = receta.IdMateriaPrima,
                NombreMateriaPrima = materiaPrima.Nombre,
                CantidadRequerida = receta.CantidadRequerida,
                IdUnidadMedida = receta.IdUnidadMedida,
                NombreUnidadMedida = unidadMedida.Nombre,
                SimboloUnidadMedida = unidadMedida.Abreviatura,
                Estado = receta.Estado
            };
        }

        /// <summary>
        /// Obtiene todas las recetas activas
        /// </summary>
        public async Task<List<RecetaProductoResponseDTO>> ObtenerRecetasAsync()
        {
            var recetas = await _recetaRepository.GetAllAsync();
            var productos = await _productosRepository.GetAllAsync();
            var materiasPrimas = await _materiaPrimaRepository.GetAllAsync();
            var unidadesMedida = await _unidadMedidaRepository.GetAllAsync();

            var productosDict = productos.ToDictionary(p => p.Id);
            var materiasPrimasDict = materiasPrimas.ToDictionary(mp => mp.Id);
            var unidadesMedidaDict = unidadesMedida.ToDictionary(um => um.Id);

            return recetas
                .Where(r => r.Estado == 1)
                .Select(r => new RecetaProductoResponseDTO
                {
                    IdReceta = r.Id,
                    IdProducto = r.IdProducto,
                    NombreProducto = productosDict.ContainsKey(r.IdProducto) ? productosDict[r.IdProducto].Nombre : "N/A",
                    IdMateriaPrima = r.IdMateriaPrima,
                    NombreMateriaPrima = materiasPrimasDict.ContainsKey(r.IdMateriaPrima) ? materiasPrimasDict[r.IdMateriaPrima].Nombre : "N/A",
                    CantidadRequerida = r.CantidadRequerida,
                    IdUnidadMedida = r.IdUnidadMedida,
                    NombreUnidadMedida = unidadesMedidaDict.ContainsKey(r.IdUnidadMedida) ? unidadesMedidaDict[r.IdUnidadMedida].Nombre : "N/A",
                    SimboloUnidadMedida = unidadesMedidaDict.ContainsKey(r.IdUnidadMedida) ? unidadesMedidaDict[r.IdUnidadMedida].Abreviatura : "N/A",
                    Estado = r.Estado
                })
                .ToList();
        }

        /// <summary>
        /// Obtiene una receta por ID
        /// </summary>
        public async Task<RecetaProductoResponseDTO?> ObtenerRecetaPorIdAsync(int id)
        {
            var receta = await _recetaRepository.FindByIdAsync(id);
            if (receta == null)
            {
                return null;
            }

            var producto = await _productosRepository.FindByIdAsync(receta.IdProducto);
            var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(receta.IdMateriaPrima);
            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(receta.IdUnidadMedida);

            return new RecetaProductoResponseDTO
            {
                IdReceta = receta.Id,
                IdProducto = receta.IdProducto,
                NombreProducto = producto?.Nombre ?? "N/A",
                IdMateriaPrima = receta.IdMateriaPrima,
                NombreMateriaPrima = materiaPrima?.Nombre ?? "N/A",
                CantidadRequerida = receta.CantidadRequerida,
                IdUnidadMedida = receta.IdUnidadMedida,
                NombreUnidadMedida = unidadMedida?.Nombre ?? "N/A",
                SimboloUnidadMedida = unidadMedida?.Abreviatura ?? "N/A",
                Estado = receta.Estado
            };
        }

        /// <summary>
        /// Obtiene todas las materias primas de un producto (receta completa)
        /// </summary>
        public async Task<List<RecetaProductoResponseDTO>> ObtenerRecetaPorProductoAsync(int idProducto)
        {
            var recetas = _recetaRepository.GetByFilter(r => r.IdProducto == idProducto && r.Estado == 1);
            var materiasPrimas = await _materiaPrimaRepository.GetAllAsync();
            var unidadesMedida = await _unidadMedidaRepository.GetAllAsync();
            var producto = await _productosRepository.FindByIdAsync(idProducto);

            var materiasPrimasDict = materiasPrimas.ToDictionary(mp => mp.Id);
            var unidadesMedidaDict = unidadesMedida.ToDictionary(um => um.Id);

            return recetas
                .Select(r => new RecetaProductoResponseDTO
                {
                    IdReceta = r.Id,
                    IdProducto = r.IdProducto,
                    NombreProducto = producto?.Nombre ?? "N/A",
                    IdMateriaPrima = r.IdMateriaPrima,
                    NombreMateriaPrima = materiasPrimasDict.ContainsKey(r.IdMateriaPrima) ? materiasPrimasDict[r.IdMateriaPrima].Nombre : "N/A",
                    CantidadRequerida = r.CantidadRequerida,
                    IdUnidadMedida = r.IdUnidadMedida,
                    NombreUnidadMedida = unidadesMedidaDict.ContainsKey(r.IdUnidadMedida) ? unidadesMedidaDict[r.IdUnidadMedida].Nombre : "N/A",
                    SimboloUnidadMedida = unidadesMedidaDict.ContainsKey(r.IdUnidadMedida) ? unidadesMedidaDict[r.IdUnidadMedida].Abreviatura : "N/A",
                    Estado = r.Estado
                })
                .ToList();
        }

        /// <summary>
        /// Actualiza una receta existente
        /// </summary>
        public async Task<RecetaProductoResponseDTO> ActualizarRecetaAsync(int id, RecetaProductoDTO dto)
        {
            // 1. Validar DTO
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            // 2. Verificar que la receta exista
            var receta = await _recetaRepository.FindByIdAsync(id);
            if (receta == null)
            {
                throw new InvalidOperationException($"No se encontró la receta con ID {id}");
            }

            // 3. Verificar que no exista otra receta con la misma combinación (excluir la actual)
            var recetaDuplicada = _recetaRepository.GetByFilter(r =>
                r.IdProducto == dto.IdProducto &&
                r.IdMateriaPrima == dto.IdMateriaPrima &&
                r.Estado == 1 &&
                r.Id != id
            );

            if (recetaDuplicada.Any())
            {
                var producto = await _productosRepository.FindByIdAsync(dto.IdProducto);
                var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(dto.IdMateriaPrima);
                throw new InvalidOperationException(
                    $"Ya existe una receta activa para el producto '{producto?.Nombre}' con la materia prima '{materiaPrima?.Nombre}'. " +
                    "No se permiten materias primas repetidas en la misma receta."
                );
            }

            // 4. Actualizar campos
            receta.IdProducto = dto.IdProducto;
            receta.IdMateriaPrima = dto.IdMateriaPrima;
            receta.CantidadRequerida = dto.CantidadRequerida;
            receta.IdUnidadMedida = dto.IdUnidadMedida;

            // 5. Guardar cambios
            await _recetaRepository.UpdateAsync(receta);

            // 6. Retornar respuesta completa
            var producto2 = await _productosRepository.FindByIdAsync(receta.IdProducto);
            var materiaPrima2 = await _materiaPrimaRepository.FindByIdAsync(receta.IdMateriaPrima);
            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(receta.IdUnidadMedida);

            return new RecetaProductoResponseDTO
            {
                IdReceta = receta.Id,
                IdProducto = receta.IdProducto,
                NombreProducto = producto2?.Nombre ?? "N/A",
                IdMateriaPrima = receta.IdMateriaPrima,
                NombreMateriaPrima = materiaPrima2?.Nombre ?? "N/A",
                CantidadRequerida = receta.CantidadRequerida,
                IdUnidadMedida = receta.IdUnidadMedida,
                NombreUnidadMedida = unidadMedida?.Nombre ?? "N/A",
                SimboloUnidadMedida = unidadMedida?.Abreviatura ?? "N/A",
                Estado = receta.Estado
            };
        }

        /// <summary>
        /// Actualiza el estado de una receta
        /// </summary>
        public async Task<bool> ActualizarEstadoRecetaAsync(int id, int nuevoEstado)
        {
            var receta = await _recetaRepository.FindByIdAsync(id);
            if (receta == null)
            {
                return false;
            }

            receta.Estado = nuevoEstado;
            await _recetaRepository.UpdateAsync(receta);
            return true;
        }

        /// <summary>
        /// Elimina (desactiva) una receta
        /// </summary>
        public async Task<bool> EliminarRecetaAsync(int id)
        {
            var receta = await _recetaRepository.FindByIdAsync(id);
            if (receta == null)
            {
                return false;
            }

            receta.Estado = 0;
            await _recetaRepository.UpdateAsync(receta);
            return true;
        }
    }
}
