using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services
{
    public class ProveedoresService(IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository)
    {
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;

        public async Task CrearProveedorAsync(ProveedoresDTO proveedoresDTO)
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

        public async Task ActualizarEstadoProveedor(ActualizarEstadoTipoGuidDTO actualizarEstadoDTO)
        {
            var proveedor = await _proveedoresRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (proveedor != null)
            {
                proveedor.Estado = actualizarEstadoDTO.EstadoNuevo;
                proveedor.FechaActualizacion = DateTime.Now;
                await _proveedoresRepository.UpdateAsync(proveedor);
            }
            else
            {
                throw new Exception("El proveedor no existe");
            }
        }

        public async Task ActualizarProveedor(Guid idProveedor, ProveedoresDTO proveedoresDTO)
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
    }
}
