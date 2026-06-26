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

namespace DistribuidoraLaVilla.Application.Services
{
    public class ProveedoresService(IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository, IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearProveedorAsync(ProveedoresDTO proveedoresDTO, Guid idUsuario)
        {
            ProveedoresEntity proveedoresEntity = new()
            {
                IdProveedor = Guid.NewGuid(),
                Nombre = proveedoresDTO.Nombre,
                Telefono = proveedoresDTO.Telefono,
                Email = proveedoresDTO.Email,
                TipoProveedor = proveedoresDTO.TipoProveedor,
                Estado = 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            await _proveedoresRepository.CreateAsync(proveedoresEntity);
            await RegistrarAuditoriaAsync("Proveedor", proveedoresEntity.IdProveedor.ToString(), "Crear", new { nombre = proveedoresEntity.Nombre, email = proveedoresEntity.Email, tipoProveedor = proveedoresEntity.TipoProveedor }, idUsuario);
        }
        public async Task<List<ProveedoresEntity>> ObtenerProveedoresAsync()
        {
            return await _proveedoresRepository.GetAllAsync();
        }

        public async Task<ProveedoresEntity> ObtenerProveedorPorIdAsync(Guid id)
        {
            var proveedor = await _proveedoresRepository.FindByIdAsync(id);
            return proveedor ?? throw new Exception("El proveedor no existe");
        }

        public async Task ActualizarEstadoProveedor(ActualizarEstadoTipoGuidDTO actualizarEstadoDTO, Guid idUsuario)
        {
            var proveedor = await _proveedoresRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (proveedor != null)
            {
                proveedor.Estado = actualizarEstadoDTO.EstadoNuevo;
                proveedor.FechaActualizacion = DateTime.Now;
                await _proveedoresRepository.UpdateAsync(proveedor);
                await RegistrarAuditoriaAsync("Proveedor", proveedor.IdProveedor.ToString(), "CambioEstado", new { estadoNuevo = proveedor.Estado }, idUsuario);
            }
            else
            {
                throw new Exception("El proveedor no existe");
            }
        }

        public async Task ActualizarProveedor(Guid idProveedor, ProveedoresDTO proveedoresDTO, Guid idUsuario)
        {
            var proveedor = await _proveedoresRepository.FindByIdAsync(idProveedor);
            if (proveedor != null)
            {
                proveedor.Nombre = proveedoresDTO.Nombre;
                proveedor.Telefono = proveedoresDTO.Telefono;
                proveedor.Email = proveedoresDTO.Email;
                proveedor.TipoProveedor = proveedoresDTO.TipoProveedor;
                proveedor.FechaActualizacion = DateTime.Now;
                await _proveedoresRepository.UpdateAsync(proveedor);
                await RegistrarAuditoriaAsync("Proveedor", proveedor.IdProveedor.ToString(), "Modificar", new { nombre = proveedor.Nombre, email = proveedor.Email, tipoProveedor = proveedor.TipoProveedor }, idUsuario);
            }
            else
            {
                throw new Exception("El proveedor no existe");
            }
        }
        public async Task<List<ProveedoresEntity>> ObtnenerProveedoresDisponibles()
        {
            var proveedores = await _proveedoresRepository.GetAllAsync();
            var proveedoresDisponibles = proveedores.Where(item => item.Estado == 1).ToList();
            return proveedoresDisponibles;
        }

        public async Task EliminarProveedorAsync(Guid idProveedor, Guid idUsuario)
        {
            var existente = await _proveedoresRepository.FindByIdAsync(idProveedor);
            if (existente != null)
            {
                await _proveedoresRepository.DeleteAsync(idProveedor);
                await RegistrarAuditoriaAsync("Proveedor", idProveedor.ToString(), "Eliminar", new { nombre = existente.Nombre }, idUsuario);
            }
            else
            {
                throw new Exception("El proveedor no existe");
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
