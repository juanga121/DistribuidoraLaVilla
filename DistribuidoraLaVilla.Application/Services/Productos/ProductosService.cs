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
    public class ProductosService(IGenericRepository<ProductosEntity, int> productos)
    {
        private readonly IGenericRepository<ProductosEntity, int> _productos = productos;

        public async Task CrearProductoAsync(ProductoDTO productoDTO)
        {
            ProductosEntity productoEntity = new()
            {
                Nombre = productoDTO.Nombre,
                Descripcion = productoDTO.Descripcion,
                IdCategoria = productoDTO.IdCategoria,
                PrecioUnitario = productoDTO.PrecioUnitario,
                Estado = 1
            };
            await _productos.CreateAsync(productoEntity);
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

        public async Task ActualizarEstadoProducto(ProductoActualizarEstadoDTO productoActualizarEstadoDTO)
        {
            var producto = await _productos.FindByIdAsync(productoActualizarEstadoDTO.Id);
            if (producto != null)
            {
                producto.Estado = productoActualizarEstadoDTO.EstadoNuevo;
                await _productos.UpdateAsync(producto);
            }
            else
            {
                throw new Exception("El producto no existe");
            }
        }

        public async Task ActualizarProducto(int idProducto, ProductoDTO productoDTO)
        {
            var producto = await _productos.FindByIdAsync(idProducto);
            if (producto != null)
            {
                producto.Nombre = productoDTO.Nombre;
                producto.Descripcion = productoDTO.Descripcion;
                producto.IdCategoria = productoDTO.IdCategoria;
                producto.PrecioUnitario = productoDTO.PrecioUnitario;
                await _productos.UpdateAsync(producto);
            }
            else
            {
                throw new Exception("Error al actualizar el producto");
            }
        }

        public async Task<List<ProductosEntity>> ObtenerProductosDisponibles()
        {
            var productos = await _productos.GetAllAsync();
            var productosDisponibles = productos.Where(item => item.Estado == 1).ToList();
            return productosDisponibles;
        }

        public async Task EliminarProductoAsync(int idProducto)
        {
            var existente = await _productos.FindByIdAsync(idProducto);
            if (existente != null)
            {
                await _productos.DeleteAsync(idProducto);
            }
            else
            {
                throw new Exception("El producto no existe");
            }
        }
    }
}
