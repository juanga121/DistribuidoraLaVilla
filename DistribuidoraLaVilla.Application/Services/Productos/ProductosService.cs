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
    public class ProductosService(
        IGenericRepository<ProductosEntity, int> productos,
        IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<ProductosEntity, int> _productos = productos;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearProductoAsync(ProductoDTO productoDTO, Guid idUsuario)
        {
            ValidarDatosProducto(productoDTO);

            var precioPorKilo = productoDTO.VentaPorPeso ? productoDTO.PrecioPorKilo : null;
            var pesoPorUnidad = productoDTO.VentaPorPeso ? productoDTO.PesoPorUnidad : null;

            ProductosEntity productoEntity = new()
            {
                Nombre = productoDTO.Nombre,
                Descripcion = productoDTO.Descripcion,
                IdCategoria = productoDTO.IdCategoria,
                PrecioUnitario = productoDTO.PrecioUnitario,
                VentaPorPeso = productoDTO.VentaPorPeso,
                PrecioPorKilo = precioPorKilo,
                PesoPorUnidad = pesoPorUnidad,
                Estado = 1
            };
            await _productos.CreateAsync(productoEntity);

            try
            {
                var detalle = JsonSerializer.Serialize(new
                {
                    nombre = productoEntity.Nombre,
                    precio = productoEntity.PrecioUnitario,
                    categoria = productoEntity.IdCategoria
                });
                await _auditoriaService.RegistrarAsync("Producto", productoEntity.Id.ToString(), "Crear", detalle, idUsuario);
            }
            catch { /* fire-and-forget: audit failure must not break the main operation */ }
        }

        public async Task<List<ProductosEntity>> ObtenerProductosAsync()
        {
            return await _productos.GetAllAsync();
        }

        public async Task<ProductosEntity> ObtenerProductoPorId(int idProducto)
        {
            var result = await _productos.FindByIdAsync(idProducto) ?? throw new Exception("No se encontro el producto");
            return result;
        }

        public async Task ActualizarEstadoProducto(ProductoActualizarEstadoDTO productoActualizarEstadoDTO, Guid idUsuario)
        {
            var producto = await _productos.FindByIdAsync(productoActualizarEstadoDTO.Id);
            if (producto != null)
            {
                var estadoAnterior = producto.Estado;
                producto.Estado = productoActualizarEstadoDTO.EstadoNuevo;
                await _productos.UpdateAsync(producto);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        estadoAnterior,
                        estadoNuevo = producto.Estado
                    });
                    await _auditoriaService.RegistrarAsync("Producto", productoActualizarEstadoDTO.Id.ToString(), "Modificar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El producto no existe");
            }
        }

        public async Task ActualizarProducto(int idProducto, ProductoDTO productoDTO, Guid idUsuario)
        {
            ValidarDatosProducto(productoDTO);

            var precioPorKilo = productoDTO.VentaPorPeso ? productoDTO.PrecioPorKilo : null;
            var pesoPorUnidad = productoDTO.VentaPorPeso ? productoDTO.PesoPorUnidad : null;

            var producto = await _productos.FindByIdAsync(idProducto);
            if (producto != null)
            {
                var precioAnterior = producto.PrecioUnitario;
                var precioPorKiloAnterior = producto.PrecioPorKilo;

                producto.Nombre = productoDTO.Nombre;
                producto.Descripcion = productoDTO.Descripcion;
                producto.IdCategoria = productoDTO.IdCategoria;
                producto.PrecioUnitario = productoDTO.PrecioUnitario;
                producto.VentaPorPeso = productoDTO.VentaPorPeso;
                producto.PrecioPorKilo = precioPorKilo;
                producto.PesoPorUnidad = pesoPorUnidad;
                await _productos.UpdateAsync(producto);

                try
                {
                    if (precioAnterior != productoDTO.PrecioUnitario || precioPorKiloAnterior != productoDTO.PrecioPorKilo)
                    {
                        var detallePrecio = JsonSerializer.Serialize(new
                        {
                            precioAnterior,
                            precioNuevo = productoDTO.PrecioUnitario,
                            precioPorKiloAnterior,
                            precioPorKiloNuevo = productoDTO.PrecioPorKilo
                        });
                        await _auditoriaService.RegistrarAsync("Producto", idProducto.ToString(), "CambioPrecio", detallePrecio, idUsuario);
                    }
                    else
                    {
                        var detalle = JsonSerializer.Serialize(new
                        {
                            nombre = productoDTO.Nombre,
                            descripcion = productoDTO.Descripcion,
                            categoria = productoDTO.IdCategoria
                        });
                        await _auditoriaService.RegistrarAsync("Producto", idProducto.ToString(), "Modificar", detalle, idUsuario);
                    }
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("Error al actualizar el producto");
            }
        }

        private static void ValidarDatosProducto(ProductoDTO productoDTO)
        {
            if (productoDTO.PrecioUnitario <= 0)
                throw new ArgumentException("El precio por unidad es obligatorio y debe ser mayor a 0");

            if (!productoDTO.VentaPorPeso)
                return;

            if (productoDTO.PrecioPorKilo == null || productoDTO.PrecioPorKilo <= 0)
                throw new ArgumentException("El precio por kilo es obligatorio y debe ser mayor a 0 cuando el producto se vende por peso");

            if (productoDTO.PesoPorUnidad == null || productoDTO.PesoPorUnidad <= 0)
                throw new ArgumentException("El peso por unidad es obligatorio y debe ser mayor a 0 cuando el producto se vende por peso");

            if (productoDTO.PrecioUnitario == productoDTO.PrecioPorKilo)
                throw new ArgumentException("El precio por unidad y el precio por kilo deben ser distintos");
        }

        public async Task<List<ProductosEntity>> ObtenerProductosDisponibles()
        {
            var productos = await _productos.GetAllAsync();
            var productosDisponibles = productos.Where(item => item.Estado == 1).ToList();
            return productosDisponibles;
        }

        public async Task EliminarProductoAsync(int idProducto, Guid idUsuario)
        {
            var existente = await _productos.FindByIdAsync(idProducto);
            if (existente != null)
            {
                await _productos.DeleteAsync(idProducto);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        nombre = existente.Nombre
                    });
                    await _auditoriaService.RegistrarAsync("Producto", idProducto.ToString(), "Eliminar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El producto no existe");
            }
        }
    }
}
