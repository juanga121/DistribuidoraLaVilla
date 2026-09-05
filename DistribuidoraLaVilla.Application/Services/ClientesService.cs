using System.Text.Json;
using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services
{
    public class ClientesService(
        IGenericRepository<ClientesEntity, Guid> clientesRepository,
        IAuditoriaService auditoriaService)
    {
        private readonly IGenericRepository<ClientesEntity, Guid> _clientesRepository = clientesRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;

        public async Task CrearClienteAsync(ClientesDTO clientesDTO, Guid idUsuario)
        {
            ClientesEntity clientesEntity = new()
            {
                IdCliente = Guid.NewGuid(),
                Nombre = clientesDTO.Nombre,
                Documento = clientesDTO.Documento,
                Direccion = clientesDTO.Direccion,
                Telefono = clientesDTO.Telefono,
                Email = clientesDTO.Email,
                LimiteCredito = clientesDTO.LimiteCredito,
                DiasCredito = clientesDTO.DiasCredito,
                TipoPersona = clientesDTO.TipoPersona ?? (int)TipoPersona.Natural,
                Estado = 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            await _clientesRepository.CreateAsync(clientesEntity);

            try
            {
                var detalle = JsonSerializer.Serialize(new
                {
                    nombre = clientesEntity.Nombre,
                    documento = clientesEntity.Documento,
                    email = clientesEntity.Email
                });
                await _auditoriaService.RegistrarAsync("Cliente", clientesEntity.IdCliente.ToString(), "Crear", detalle, idUsuario);
            }
            catch { /* fire-and-forget */ }
        }
        public async Task<List<ClientesEntity>> ObtenerClientesAsync()
        {
            return await _clientesRepository.GetAllAsync();
        }

        public async Task<ClientesEntity> ObtenerClientePorIdAsync(Guid id)
        {
            var cliente = await _clientesRepository.FindByIdAsync(id);
            return cliente ?? throw new Exception("El cliente no existe");
        }

        public async Task ActualizarEstadoCliente(ActualizarEstadoTipoGuidDTO actualizarEstadoDTO, Guid idUsuario)
        {
            var cliente = await _clientesRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (cliente != null)
            {
                var estadoAnterior = cliente.Estado;
                cliente.Estado = actualizarEstadoDTO.EstadoNuevo;
                cliente.FechaActualizacion = DateTime.Now;
                await _clientesRepository.UpdateAsync(cliente);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        estadoAnterior,
                        estadoNuevo = cliente.Estado
                    });
                    await _auditoriaService.RegistrarAsync("Cliente", actualizarEstadoDTO.Id.ToString(), "Modificar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El cliente no existe");
            }
        }

        public async Task ActualizarCliente(Guid idCliente, ClientesDTO clientesDTO, Guid idUsuario)
        {
            var cliente = await _clientesRepository.FindByIdAsync(idCliente);
            if (cliente != null)
            {
                cliente.Nombre = clientesDTO.Nombre;
                cliente.Documento = clientesDTO.Documento;
                cliente.Direccion = clientesDTO.Direccion;
                cliente.Telefono = clientesDTO.Telefono;
                cliente.Email = clientesDTO.Email;
                cliente.LimiteCredito = clientesDTO.LimiteCredito;
                cliente.DiasCredito = clientesDTO.DiasCredito;
                cliente.TipoPersona = clientesDTO.TipoPersona ?? (int)TipoPersona.Natural;
                cliente.FechaActualizacion = DateTime.Now;
                await _clientesRepository.UpdateAsync(cliente);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        nombre = clientesDTO.Nombre,
                        documento = clientesDTO.Documento,
                        email = clientesDTO.Email
                    });
                    await _auditoriaService.RegistrarAsync("Cliente", idCliente.ToString(), "Modificar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El cliente no existe");
            }
        }
        public async Task<List<ClientesEntity>> ObtenerClientesDisponibles()
        {
            var clientes = await _clientesRepository.GetAllAsync();
            var clientesDisponibles = clientes.Where(item => item.Estado == 1).ToList();
            return clientesDisponibles;
        }

        public async Task EliminarClienteAsync(Guid idCliente, Guid idUsuario)
        {
            var existente = await _clientesRepository.FindByIdAsync(idCliente);
            if (existente != null)
            {
                await _clientesRepository.DeleteAsync(idCliente);

                try
                {
                    var detalle = JsonSerializer.Serialize(new
                    {
                        nombre = existente.Nombre
                    });
                    await _auditoriaService.RegistrarAsync("Cliente", idCliente.ToString(), "Eliminar", detalle, idUsuario);
                }
                catch { /* fire-and-forget */ }
            }
            else
            {
                throw new Exception("El cliente no existe");
            }
        }
    }
}
