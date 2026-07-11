using DistribuidoraLaVilla.Application.Validators;
using DistribuidoraLaVilla.Domain.DTOS.Compras;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Compras;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;
using FluentValidation;

namespace DistribuidoraLaVilla.Application.Services.Compras
{
    public class OrdenCompraService(
        IGenericRepository<OrdenCompraEntity, int> ordenCompraRepository,
        IGenericRepository<DetalleCompraEntity, int> detalleRepository,
        IGenericRepository<ProveedoresEntity, Guid> proveedorRepository,
        IGenericRepository<ProductosEntity, int> productoRepository,
        IUnitOfWork unitOfWork)
    {
        private readonly IGenericRepository<OrdenCompraEntity, int> _ordenCompraRepository = ordenCompraRepository;
        private readonly IGenericRepository<DetalleCompraEntity, int> _detalleRepository = detalleRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedorRepository = proveedorRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productoRepository = productoRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CrearOrdenCompraDTOValidator _validator = new();

        public async Task<OrdenCompraResponseDTO> CrearOrdenCompraAsync(CrearOrdenCompraDTO dto, Guid idUsuario)
        {
            // 1. Validar con FluentValidation
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errores);
            }

            var proveedor = await _proveedorRepository.FindByIdAsync(dto.IdProveedor)
                ?? throw new KeyNotFoundException($"No se encontró el proveedor con ID {dto.IdProveedor}");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Calcular subtotal total
                decimal subtotalTotal = 0;
                foreach (var det in dto.Detalles)
                {
                    subtotalTotal += det.Cantidad * det.PrecioUnitario;
                }

                var orden = new OrdenCompraEntity
                {
                    IdProveedor = dto.IdProveedor,
                    FechaEmision = dto.FechaEmision,
                    Estado = (int)EstadoCompraEnum.Pendiente,
                    Subtotal = subtotalTotal,
                    Descuento = 0,
                    Impuesto = 0,
                    Total = subtotalTotal,
                    Observaciones = dto.Observaciones,
                    FechaCreacion = DateTime.Now
                };

                await _ordenCompraRepository.CreateAsync(orden);

                foreach (var det in dto.Detalles)
                {
                    var producto = await _productoRepository.FindByIdAsync(det.IdProducto)
                        ?? throw new KeyNotFoundException($"No se encontró el producto con ID {det.IdProducto}");

                    var detalle = new DetalleCompraEntity
                    {
                        IdOrdenCompra = orden.Id,
                        IdProducto = det.IdProducto,
                        Cantidad = det.Cantidad,
                        PrecioUnitario = det.PrecioUnitario,
                        Subtotal = det.Cantidad * det.PrecioUnitario
                    };

                    await _detalleRepository.CreateAsync(detalle);
                }

                await _unitOfWork.CommitAsync();

                return MapToResponse(orden, proveedor.Nombre);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrdenCompraResponseDTO>> ObtenerOrdenesCompraAsync()
        {
            var ordenes = await _ordenCompraRepository.GetAllAsync();
            var resultado = new List<OrdenCompraResponseDTO>();

            foreach (var orden in ordenes)
            {
                var nombreProveedor = await ObtenerNombreProveedorAsync(orden.IdProveedor);
                resultado.Add(MapToResponse(orden, nombreProveedor));
            }

            return resultado;
        }

        public async Task<OrdenCompraResponseDTO?> ObtenerOrdenCompraPorIdAsync(int id)
        {
            var orden = await _ordenCompraRepository.FindByIdAsync(id);
            if (orden == null) return null;

            var nombreProveedor = await ObtenerNombreProveedorAsync(orden.IdProveedor);
            var response = MapToResponse(orden, nombreProveedor);

            var detallesEntity = _detalleRepository.GetByFilter(d => d.IdOrdenCompra == id);
            var detallesDTO = new List<DetalleCompraResponseDTO>();

            foreach (var det in detallesEntity)
            {
                var nombreProducto = await ObtenerNombreProductoAsync(det.IdProducto);
                detallesDTO.Add(new DetalleCompraResponseDTO
                {
                    Id = det.Id,
                    IdProducto = det.IdProducto,
                    NombreProducto = nombreProducto,
                    Cantidad = det.Cantidad,
                    PrecioUnitario = det.PrecioUnitario,
                    Subtotal = det.Subtotal
                });
            }

            response.Detalles = detallesDTO;
            return response;
        }

        public async Task ActualizarEstadoOrdenAsync(int id, int nuevoEstado, Guid idUsuario)
        {
            var orden = await _ordenCompraRepository.FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró la orden de compra con ID {id}");

            ValidarTransicionEstado(orden.Estado, nuevoEstado);

            orden.Estado = nuevoEstado;
            orden.FechaActualizacion = DateTime.Now;

            if (nuevoEstado == (int)EstadoCompraEnum.Recibida)
            {
                orden.FechaRecepcion = DateTime.Now;
            }

            await _ordenCompraRepository.UpdateAsync(orden);
        }

        private static void ValidarTransicionEstado(int desde, int hacia)
        {
            bool valida = (desde, hacia) switch
            {
                (1, 2) => true,  // Pendiente → Aprobada
                (1, 4) => true,  // Pendiente → Cancelada
                (2, 3) => true,  // Aprobada → Recibida
                _ => false
            };

            if (!valida)
            {
                throw new InvalidOperationException(
                    $"Transición de estado inválida: {desde} → {hacia}");
            }
        }

        private async Task<string?> ObtenerNombreProveedorAsync(Guid idProveedor)
        {
            var proveedor = await _proveedorRepository.FindByIdAsync(idProveedor);
            return proveedor?.Nombre;
        }

        private async Task<string?> ObtenerNombreProductoAsync(int idProducto)
        {
            var producto = await _productoRepository.FindByIdAsync(idProducto);
            return producto?.Nombre;
        }

        private static OrdenCompraResponseDTO MapToResponse(OrdenCompraEntity e, string? nombreProveedor) => new()
        {
            Id = e.Id,
            IdProveedor = e.IdProveedor,
            NombreProveedor = nombreProveedor,
            FechaEmision = e.FechaEmision,
            FechaRecepcion = e.FechaRecepcion,
            Estado = e.Estado,
            NombreEstado = ObtenerNombreEstado(e.Estado),
            Subtotal = e.Subtotal,
            Descuento = e.Descuento,
            Impuesto = e.Impuesto,
            Total = e.Total,
            Observaciones = e.Observaciones
        };

        private static string? ObtenerNombreEstado(int estado)
        {
            return estado switch
            {
                1 => "Pendiente",
                2 => "Aprobada",
                3 => "Recibida",
                4 => "Cancelada",
                _ => null
            };
        }
    }
}
