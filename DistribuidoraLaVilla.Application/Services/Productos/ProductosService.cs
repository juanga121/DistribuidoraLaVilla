using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class ProductosService(
        IGenericRepository<ProductosEntity, int> productos,
        IAuditoriaService auditoriaService,
        IGenericRepository<LotesProductosEntity, int> lotesProductos)
    {
        private readonly IGenericRepository<ProductosEntity, int> _productos = productos;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository = lotesProductos;

        public async Task CrearProductoAsync(ProductoDTO productoDTO, Guid idUsuario)
        {
            var modo = productoDTO.ModoInventario == 0
                ? (productoDTO.VentaPorPeso ? (int)ModoInventarioEnum.Peso : (int)ModoInventarioEnum.Unidad)
                : productoDTO.ModoInventario;

            ValidarDatosProducto(productoDTO, modo);

            var modoUnidad = modo == (int)ModoInventarioEnum.Unidad;
            var modoMixto = modo == (int)ModoInventarioEnum.UnidadYPeso;
            var ventaPorPeso = modo == (int)ModoInventarioEnum.Peso || modoMixto;

            var precioPorKilo = ventaPorPeso ? productoDTO.PrecioPorKilo : null;
            // Solo el modo mixto usa el peso por unidad y el precio por unidad junto al kilo.
            var pesoPorUnidad = modoMixto ? productoDTO.PesoPorUnidad : null;
            var precioUnitario = modoUnidad || modoMixto ? productoDTO.PrecioUnitario : 0m;

            ProductosEntity productoEntity = new()
            {
                Nombre = productoDTO.Nombre,
                Descripcion = productoDTO.Descripcion,
                IdCategoria = productoDTO.IdCategoria,
                PrecioUnitario = precioUnitario,
                VentaPorPeso = ventaPorPeso,
                PrecioPorKilo = precioPorKilo,
                PesoPorUnidad = pesoPorUnidad,
                ModoInventario = modo,
                StockMinimo = productoDTO.StockMinimo,
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
            return await ConIndicadorDeLotesAsync(await _productos.GetAllAsync());
        }

        public async Task<ProductosEntity> ObtenerProductoPorId(int idProducto)
        {
            var result = await _productos.FindByIdAsync(idProducto) ?? throw new Exception("No se encontro el producto");
            await MarcarLotesAsync(new List<ProductosEntity> { result });
            return result;
        }

        /// <summary>
        /// Marca cada producto con si ya tiene lotes. El front lo usa para impedir cambiar el modo
        /// de inventario de un producto que ya tiene stock.
        /// </summary>
        private async Task<List<ProductosEntity>> ConIndicadorDeLotesAsync(List<ProductosEntity> productos)
        {
            await MarcarLotesAsync(productos);
            return productos;
        }

        private Task MarcarLotesAsync(List<ProductosEntity> productos)
        {
            if (productos.Count == 0)
            {
                return Task.CompletedTask;
            }

            var conLotes = _lotesProductosRepository
                .GetByFilter(l => l.IdProducto > 0)
                .Select(l => l.IdProducto)
                .ToHashSet();

            foreach (var producto in productos)
            {
                producto.TieneLotes = conLotes.Contains(producto.Id);
            }

            return Task.CompletedTask;
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
            var modo = productoDTO.ModoInventario == 0
                ? (productoDTO.VentaPorPeso ? (int)ModoInventarioEnum.Peso : (int)ModoInventarioEnum.Unidad)
                : productoDTO.ModoInventario;

            ValidarDatosProducto(productoDTO, modo);

            var modoUnidad = modo == (int)ModoInventarioEnum.Unidad;
            var modoMixto = modo == (int)ModoInventarioEnum.UnidadYPeso;
            var ventaPorPeso = modo == (int)ModoInventarioEnum.Peso || modoMixto;

            var precioPorKilo = ventaPorPeso ? productoDTO.PrecioPorKilo : null;
            // Solo el modo mixto usa el peso por unidad y el precio por unidad junto al kilo.
            var pesoPorUnidad = modoMixto ? productoDTO.PesoPorUnidad : null;
            var precioUnitario = modoUnidad || modoMixto ? productoDTO.PrecioUnitario : 0m;

            var producto = await _productos.FindByIdAsync(idProducto);
            if (producto != null)
            {
                if (producto.ModoInventario != modo && _lotesProductosRepository.GetByFilter(l => l.IdProducto == idProducto).Count > 0)
                    throw new InvalidOperationException("No se puede cambiar el modo de inventario de un producto que ya tiene movimientos de stock");

                var precioAnterior = producto.PrecioUnitario;
                var precioPorKiloAnterior = producto.PrecioPorKilo;

                producto.Nombre = productoDTO.Nombre;
                producto.Descripcion = productoDTO.Descripcion;
                producto.IdCategoria = productoDTO.IdCategoria;
                producto.PrecioUnitario = precioUnitario;
                producto.VentaPorPeso = ventaPorPeso;
                producto.PrecioPorKilo = precioPorKilo;
                producto.PesoPorUnidad = pesoPorUnidad;
                producto.ModoInventario = modo;
                producto.StockMinimo = productoDTO.StockMinimo;
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

        private static void ValidarDatosProducto(ProductoDTO productoDTO, int modo)
        {
            var modoValido = modo == (int)ModoInventarioEnum.Unidad
                || modo == (int)ModoInventarioEnum.Peso
                || modo == (int)ModoInventarioEnum.UnidadYPeso;

            if (!modoValido)
                throw new ArgumentException("El modo de inventario debe ser 1 (Unidad), 2 (Peso) o 4 (Unidad y peso)");

            if (productoDTO.StockMinimo.HasValue && productoDTO.StockMinimo.Value < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo");

            if (modo == (int)ModoInventarioEnum.Unidad)
            {
                if (productoDTO.PrecioUnitario <= 0)
                    throw new ArgumentException("El precio por unidad es obligatorio y debe ser mayor a 0");

                return;
            }

            if (productoDTO.PrecioPorKilo == null || productoDTO.PrecioPorKilo <= 0)
                throw new ArgumentException("El precio por kilo es obligatorio y debe ser mayor a 0 cuando el producto se vende por peso");

            // El modo mixto necesita precio por kilo y por unidad. El peso por unidad es opcional:
            // si está, se usa como sugerencia; si no, el cajero pesa al vender por unidad.
            if (modo == (int)ModoInventarioEnum.UnidadYPeso)
            {
                if (productoDTO.PesoPorUnidad.HasValue && productoDTO.PesoPorUnidad <= 0)
                    throw new ArgumentException("El peso sugerido por unidad debe ser mayor a 0 cuando se informa");

                if (productoDTO.PrecioUnitario <= 0)
                    throw new ArgumentException("El precio por unidad es obligatorio para un producto que se vende por unidad y por peso");

                if (productoDTO.PrecioUnitario == productoDTO.PrecioPorKilo)
                    throw new ArgumentException("El precio por unidad y el precio por kilo deben ser distintos");
            }

            // En modo Peso no se usan ni el precio por unidad ni el peso por unidad.
        }

        public async Task<List<ProductosEntity>> ObtenerProductosDisponibles()
        {
            var productos = await _productos.GetAllAsync();
            var productosDisponibles = productos.Where(item => item.Estado == 1).ToList();
            await MarcarLotesAsync(productosDisponibles);
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
