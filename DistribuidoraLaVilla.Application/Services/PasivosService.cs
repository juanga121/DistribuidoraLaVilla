using System.Text.Json;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class PasivosService(IGenericRepository<PasivosEntity, int> pasivosRepository, MovimientosFinancierosService movimientosService)
    {
        private readonly IGenericRepository<PasivosEntity, int> _pasivosRepository = pasivosRepository;
        private readonly MovimientosFinancierosService _movimientosService = movimientosService;

        public async Task CrearPasivoAsync(PasivosDTO pasivosDTO, Guid idUsuario)
        {
            var pasivo = new PasivosEntity
            {
                Nombre = pasivosDTO.Nombre,
                Descripcion = pasivosDTO.Descripcion,
                Monto = pasivosDTO.Monto,
                Estado = pasivosDTO.Estado ?? 1,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
            };

            await _pasivosRepository.CreateAsync(pasivo);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "Pasivo",
                SubTipo = "Creacion",
                Descripcion = $"Creación de pasivo: {pasivo.Nombre}",
                Monto = pasivo.Monto,
                Direccion = "Egreso",
                OrigenModulo = "Pasivos",
                ReferenciaId = pasivo.Id,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, idUsuario);
        }

        public async Task<List<PasivosEntity>> ObtenerPasivosAsync()
        {
            return await _pasivosRepository.GetAllAsync();
        }

        public async Task<PasivosEntity> ObtenerPasivoPorIdAsync(int id)
        {
            var pasivo = await _pasivosRepository.FindByIdAsync(id);
            return pasivo ?? throw new Exception("El pasivo no existe");
        }

        public async Task ActualizarPasivoAsync(int id, PasivosDTO pasivosDTO, Guid idUsuario)
        {
            var pasivo = await _pasivosRepository.FindByIdAsync(id);
            if (pasivo == null)
            {
                throw new Exception("El pasivo no existe");
            }

            pasivo.Nombre = pasivosDTO.Nombre;
            pasivo.Descripcion = pasivosDTO.Descripcion;
            pasivo.Monto = pasivosDTO.Monto;
            pasivo.Estado = pasivosDTO.Estado ?? pasivo.Estado;
            pasivo.FechaActualizacion = DateTime.Now;
            pasivo.IdUsuario = idUsuario == Guid.Empty ? pasivo.IdUsuario : idUsuario;

            await _pasivosRepository.UpdateAsync(pasivo);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "Pasivo",
                SubTipo = "Ajuste",
                Descripcion = $"Ajuste de pasivo: {pasivo.Nombre}",
                Monto = pasivo.Monto,
                Direccion = "Neutro",
                OrigenModulo = "Pasivos",
                ReferenciaId = pasivo.Id,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, idUsuario);
        }

        public async Task EliminarPasivoAsync(int id)
        {
            var existente = await _pasivosRepository.FindByIdAsync(id);
            if (existente == null)
            {
                throw new Exception("El pasivo no existe");
            }

            await _pasivosRepository.DeleteAsync(id);

            await _movimientosService.CrearMovimientoAsync(new CrearMovimientoFinancieroDTO
            {
                TipoMovimiento = "Pasivo",
                SubTipo = "Baja",
                Descripcion = $"Baja de pasivo: {existente.Nombre}",
                Monto = existente.Monto,
                Direccion = "Ingreso",
                OrigenModulo = "Pasivos",
                ReferenciaId = existente.Id,
                FechaMovimiento = DateTime.Now,
                Estado = 1
            }, existente.IdUsuario ?? Guid.Empty);
        }
    }
}
