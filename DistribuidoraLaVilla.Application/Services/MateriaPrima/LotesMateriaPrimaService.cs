using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class LotesMateriaPrimaService(IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository)
    {
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository = lotesMateriaPrimaRepository;

        public async Task CrearLoteMateriaPrimaAsync(LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            var CostoTotal = CalculoCostoTotal(lotesMateriaPrimaDTO.Cantidad, lotesMateriaPrimaDTO.CostoUnitario);

            LotesMateriaPrimaEntity entity = new()
            {
                IdMarca = lotesMateriaPrimaDTO.IdMarca,
                IdMateria = lotesMateriaPrimaDTO.IdMateria,
                IdProveedor = lotesMateriaPrimaDTO.IdProveedor,
                FechaEntrada = DateTime.Now,
                FechaVencimiento = lotesMateriaPrimaDTO.FechaVencimiento,
                Cantidad = lotesMateriaPrimaDTO.Cantidad,
                IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida,
                CostoUnitario = lotesMateriaPrimaDTO.CostoUnitario,
                CostoTotal = CostoTotal,
                Estado = 1
            };
            await _lotesMateriaPrimaRepository.CreateAsync(entity);
        }

        public async Task<List<LotesMateriaPrimaEntity>> ObtenerLotesMateriaPrimaAsync()
        {
            return await _lotesMateriaPrimaRepository.GetAllAsync();
        }

        public async Task<LotesMateriaPrimaEntity> ObtenerLoteMateriaPrimaPorIdAsync(int id)
        {
            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(id);
            return lote ?? throw new Exception("El lote de materia prima no existe");
        }

        public async Task ActualizarEstadoLoteMateriaPrima(ActualizarEstadoTipoIntDTO actualizarEstadoDTO)
        {
            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(actualizarEstadoDTO.Id);
            if (lote != null)
            {
                lote.Estado = actualizarEstadoDTO.EstadoNuevo;
                await _lotesMateriaPrimaRepository.UpdateAsync(lote);
            }
            else
            {
                throw new Exception("El lote de materia prima no existe");
            }
        }

        public async Task ActualizarLoteMateriaPrima(int id, LotesMateriaPrimaDTO lotesMateriaPrimaDTO)
        {
            var lote = await _lotesMateriaPrimaRepository.FindByIdAsync(id);
            if (lote != null)
            {
                lote.IdMarca = lotesMateriaPrimaDTO.IdMarca;
                lote.IdMateria = lotesMateriaPrimaDTO.IdMateria;
                lote.IdProveedor = lotesMateriaPrimaDTO.IdProveedor;
                lote.FechaEntrada = DateTime.Now;
                lote.FechaVencimiento = lotesMateriaPrimaDTO.FechaVencimiento;
                lote.Cantidad = lotesMateriaPrimaDTO.Cantidad;
                lote.IdUnidadMedida = lotesMateriaPrimaDTO.IdUnidadMedida;
                lote.CostoUnitario = lotesMateriaPrimaDTO.CostoUnitario;
                lote.CostoTotal = CalculoCostoTotal(lotesMateriaPrimaDTO.Cantidad, lotesMateriaPrimaDTO.CostoUnitario);
                await _lotesMateriaPrimaRepository.UpdateAsync(lote);
            }
        }

        public async Task<List<LotesMateriaPrimaEntity>> ObtenerLotesMateriaPrimaDisponiblesAsync()
        {
            var lotes = await _lotesMateriaPrimaRepository.GetAllAsync();
            return [.. lotes.Where(l => l.Estado == 1)];
        }

        // New: delete lote by id using generic repository
        public async Task EliminarLoteMateriaPrimaAsync(int idLote)
        {
            var existente = await _lotesMateriaPrimaRepository.FindByIdAsync(idLote);
            if (existente != null)
            {
                await _lotesMateriaPrimaRepository.DeleteAsync(idLote);
            }
            else
            {
                throw new Exception("El lote de materia prima no existe");
            }
        }

        private static decimal CalculoCostoTotal(decimal cantidad, decimal costoUnitario)
        {
            var costoTotal = cantidad * costoUnitario;
            return costoTotal;
        }
    }
}
