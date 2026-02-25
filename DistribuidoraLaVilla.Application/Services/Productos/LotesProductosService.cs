using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.Productos
{
    public class LotesProductosService(IGenericRepository<LotesProductosEntity, int> lotesProductosRepository)
    {
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository = lotesProductosRepository;

        public async Task CrearLoteProductoAsync(LotesProductosDTO lotesProductosDTO)
        {
            var precioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);

            LotesProductosEntity entity = new()
            {
                IdProducto = lotesProductosDTO.IdProducto,
                IdProveedor = lotesProductosDTO.IdProveedor,
                FechaEntrada = DateTime.Now,
                FechaVencimiento = lotesProductosDTO.FechaVencimiento,
                CantidadUnidades = lotesProductosDTO.CantidadUnidades,
                PesoTotal = lotesProductosDTO.PesoTotal,
                IdUnidadMedida = lotesProductosDTO.IdUnidadMedida,
                PrecioUnitario = lotesProductosDTO.PrecioUnitario,
                PrecioKilo = lotesProductosDTO.PrecioKilo,
                PrecioTotal = precioTotal,
                IdMarca = lotesProductosDTO.IdMarca,
                Estado = 1
            };
            await _lotesProductosRepository.CreateAsync(entity);
        }

        public async Task<List<LotesProductosEntity>> ObtenerLotesProductosAsync()
        {
            return await _lotesProductosRepository.GetAllAsync();
        }

        public async Task<LotesProductosEntity> ObtenerLoteProductoPorIdAsync(int id)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(id);
            return lote ?? throw new Exception("El lote de producto no existe");
        }

        public async Task ActualizarEstadoLoteProducto(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (lote != null)
            {
                lote.Estado = actualizarEstadoDTO.EstadoNuevo;
                await _lotesProductosRepository.UpdateAsync(lote);
            }
            else
            {
                throw new Exception("El lote de producto no existe");
            }
        }

        public async Task ActualizarLoteProducto(int id, LotesProductosDTO lotesProductosDTO)
        {
            var lote = await _lotesProductosRepository.FindByIdAsync(id);
            if (lote != null)
            {
                lote.IdProducto = lotesProductosDTO.IdProducto;
                lote.IdProveedor = lotesProductosDTO.IdProveedor;
                lote.FechaEntrada = DateTime.Now;
                lote.FechaVencimiento = lotesProductosDTO.FechaVencimiento;
                lote.CantidadUnidades = lotesProductosDTO.CantidadUnidades;
                lote.PesoTotal = lotesProductosDTO.PesoTotal;
                lote.IdUnidadMedida = lotesProductosDTO.IdUnidadMedida;
                lote.PrecioUnitario = lotesProductosDTO.PrecioUnitario;
                lote.PrecioKilo = lotesProductosDTO.PrecioKilo;
                lote.PrecioTotal = CalculoPrecioTotal(lotesProductosDTO.CantidadUnidades, lotesProductosDTO.PrecioUnitario);
                lote.IdMarca = lotesProductosDTO.IdMarca;
                await _lotesProductosRepository.UpdateAsync(lote);
            }
        }

        public async Task<List<LotesProductosEntity>> ObtenerLotesProductosDisponiblesAsync()
        {
            var lotes = await _lotesProductosRepository.GetAllAsync();
            return [.. lotes.Where(l => l.Estado == 1)];
        }

        public async Task EliminarLoteProductoAsync(int idLote)
        {
            var existente = await _lotesProductosRepository.FindByIdAsync(idLote);
            if (existente != null)
            {
                await _lotesProductosRepository.DeleteAsync(idLote);
            }
            else
            {
                throw new Exception("El lote de producto no existe");
            }
        }

        private static decimal CalculoPrecioTotal(int cantidadUnidades, decimal precioUnitario)
        {
            var precioTotal = cantidadUnidades * precioUnitario;
            return precioTotal;
        }
    }
}
