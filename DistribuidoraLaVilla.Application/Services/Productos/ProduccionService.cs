using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Validators.Productos;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class ProduccionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventarioService _inventarioService;
        private readonly IGenericRepository<OrdenProduccionEntity, int> _ordenRepository;
        private readonly IGenericRepository<RecetaProductoEntity, int> _recetaRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository;
        private readonly IGenericRepository<MovimientosProductosEntity, int> _movimientosProductosRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;
        private readonly SolicitudProduccionDTOValidator _validator;

        public ProduccionService(
            IUnitOfWork unitOfWork,
            IInventarioService inventarioService,
            IGenericRepository<OrdenProduccionEntity, int> ordenRepository,
            IGenericRepository<RecetaProductoEntity, int> recetaRepository,
            IGenericRepository<ProductosEntity, int> productosRepository,
            IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
            IGenericRepository<MovimientosProductosEntity, int> movimientosProductosRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository)
        {
            _unitOfWork = unitOfWork;
            _inventarioService = inventarioService;
            _ordenRepository = ordenRepository;
            _recetaRepository = recetaRepository;
            _productosRepository = productosRepository;
            _lotesProductosRepository = lotesProductosRepository;
            _movimientosProductosRepository = movimientosProductosRepository;
            _unidadMedidaRepository = unidadMedidaRepository;
            _validator = new SolicitudProduccionDTOValidator();
        }

        /// <summary>
        /// Procesa una orden de producción completa.
        /// Usa transacción para garantizar atomicidad: si algo falla,
        /// se revierten todos los cambios (stock, movimientos, orden).
        /// </summary>
        public async Task<ResultadoProduccionDTO> ProcesarProduccionAsync(SolicitudProduccionDTO solicitud)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Validar entrada
                var validationResult = await _validator.ValidateAsync(solicitud);
                if (!validationResult.IsValid)
                {
                    var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException(errores);
                }

                // 2. Verificar que el producto exista
                var producto = await _productosRepository.FindByIdAsync(solicitud.IdProducto);
                if (producto == null || producto.Estado != 1)
                {
                    throw new InvalidOperationException($"No se encontró el producto con ID {solicitud.IdProducto} o está inactivo");
                }

                // 3. Obtener receta del producto
                var recetas = _recetaRepository.GetByFilter(r =>
                    r.IdProducto == solicitud.IdProducto && r.Estado == 1);

                if (!recetas.Any())
                {
                    throw new InvalidOperationException(
                        $"El producto '{producto.Nombre}' no tiene una receta configurada. " +
                        "Por favor, configure la receta antes de producir."
                    );
                }

                // 4. Calcular cantidades necesarias de cada ingrediente
                var ingredientesNecesarios = recetas.Select(r => new
                {
                    r.IdMateriaPrima,
                    CantidadPorUnidad = r.CantidadRequerida,
                    CantidadTotal = r.CantidadRequerida * solicitud.CantidadProducir,
                    r.IdUnidadMedida
                }).ToList();

                // 5. Crear orden de producción
                var orden = new OrdenProduccionEntity
                {
                    IdProducto = solicitud.IdProducto,
                    CantidadProducir = solicitud.CantidadProducir,
                    IdUnidadMedida = solicitud.IdUnidadMedida,
                    FechaOrden = DateTime.Now,
                    IdUsuario = solicitud.IdUsuario,
                    Observaciones = solicitud.Observaciones,
                    Estado = 1 // Pendiente
                };

                await _ordenRepository.CreateAsync(orden);

                // 6. Consumir materia prima de lotes usando FIFO (servicio compartido)
                var ingredientesConsumidos = new List<ConsumoIngredienteDTO>();

                foreach (var ingrediente in ingredientesNecesarios)
                {
                    var consumos = await _inventarioService.ConsumirLotesMateriaPrimaAsync(
                        idMateriaPrima: ingrediente.IdMateriaPrima,
                        cantidadRequerida: ingrediente.CantidadTotal,
                        idUnidadMedida: ingrediente.IdUnidadMedida,
                        idUsuario: solicitud.IdUsuario,
                        observacion: $"Consumo para producción de {solicitud.CantidadProducir} unidades de {producto.Nombre}"
                    );

                    ingredientesConsumidos.AddRange(consumos);
                }

                // 7. Crear lote de producto terminado
                var nuevoLoteProducto = new LotesProductosEntity
                {
                    IdProducto = solicitud.IdProducto,
                    IdProveedor = Guid.Empty, // Producción interna
                    FechaEntrada = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddMonths(6), // Default 6 meses
                    CantidadUnidades = (int)solicitud.CantidadProducir,
                    PesoTotal = solicitud.CantidadProducir,
                    IdUnidadMedida = solicitud.IdUnidadMedida,
                    PrecioUnitario = producto.PrecioUnitario,
                    PrecioKilo = producto.PrecioUnitario,
                    PrecioTotal = producto.PrecioUnitario * solicitud.CantidadProducir,
                    IdMarca = 1, // Default o configurar según negocio
                    CantidadInicial = solicitud.CantidadProducir,
                    CantidadDisponible = solicitud.CantidadProducir,
                    Estado = 1
                };

                await _lotesProductosRepository.CreateAsync(nuevoLoteProducto);

                // 8. Crear movimiento de entrada de producto
                var movimientoEntradaProducto = new MovimientosProductosEntity
                {
                    IdLoteProducto = nuevoLoteProducto.Id,
                    TipoMovimiento = (int)TipoMovimientoProducto.Entrada,
                    FechaMovimiento = DateTime.Now,
                    Cantidad = solicitud.CantidadProducir,
                    TotalMovimiento = producto.PrecioUnitario * solicitud.CantidadProducir,
                    IdUnidadMedida = solicitud.IdUnidadMedida,
                    IdCliente = null,
                    IdProveedor = null,
                    IdUsuario = solicitud.IdUsuario,
                    Observacion = $"Entrada por producción - Orden #{orden.Id}",
                    Estado = 1
                };

                await _movimientosProductosRepository.CreateAsync(movimientoEntradaProducto);

                // 9. Actualizar orden como completada
                orden.FechaCompletada = DateTime.Now;
                orden.IdLoteGenerado = nuevoLoteProducto.Id;
                orden.Estado = 2; // Completada
                await _ordenRepository.UpdateAsync(orden);

                // 10. Construir respuesta exitosa
                var unidadMedidaProducto = await _unidadMedidaRepository.FindByIdAsync(solicitud.IdUnidadMedida);

                await _unitOfWork.CommitAsync();

                return new ResultadoProduccionDTO
                {
                    Exitoso = true,
                    IdOrden = orden.Id,
                    IdProducto = producto.Id,
                    NombreProducto = producto.Nombre,
                    CantidadProducida = solicitud.CantidadProducir,
                    UnidadMedida = unidadMedidaProducto?.Abreviatura,
                    IdLoteGenerado = nuevoLoteProducto.Id,
                    IdMovimientoEntradaProducto = movimientoEntradaProducto.Id,
                    FechaProduccion = DateTime.Now,
                    IngredientesConsumidos = ingredientesConsumidos,
                    Observaciones = solicitud.Observaciones,
                    Mensaje = $"Producción exitosa: {solicitud.CantidadProducir} unidades de {producto.Nombre}"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                return new ResultadoProduccionDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error en la producción: {ex.Message}",
                    IngredientesConsumidos = new List<ConsumoIngredienteDTO>()
                };
            }
            finally
            {
                await _unitOfWork.DisposeAsync();
            }
        }

        /// <summary>
        /// Obtiene todas las órdenes de producción
        /// </summary>
        public async Task<List<OrdenProduccionEntity>> ObtenerOrdenesProduccionAsync()
        {
            return await _ordenRepository.GetAllAsync();
        }

        /// <summary>
        /// Obtiene una orden de producción por ID
        /// </summary>
        public async Task<OrdenProduccionEntity?> ObtenerOrdenPorIdAsync(int id)
        {
            return await _ordenRepository.FindByIdAsync(id);
        }

        /// <summary>
        /// Obtiene órdenes por estado (1=Pendiente, 2=Completada, 0=Cancelada)
        /// </summary>
        public List<OrdenProduccionEntity> ObtenerOrdenesPorEstado(int estado)
        {
            return _ordenRepository.GetByFilter(o => o.Estado == estado);
        }

        /// <summary>
        /// Cancela una orden de producción pendiente
        /// </summary>
        public async Task<bool> CancelarOrdenAsync(int id, Guid idUsuario)
        {
            var orden = await _ordenRepository.FindByIdAsync(id);
            if (orden == null || orden.Estado != 1)
            {
                return false;
            }

            orden.Estado = 0;
            orden.Observaciones += $" | Cancelada por usuario {idUsuario} el {DateTime.Now:yyyy-MM-dd HH:mm}";
            await _ordenRepository.UpdateAsync(orden);
            return true;
        }
    }
}
