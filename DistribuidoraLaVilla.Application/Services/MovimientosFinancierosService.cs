using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class MovimientosFinancierosService(IGenericRepository<MovimientosFinancierosEntity, int> movimientosRepository)
    {
        private readonly IGenericRepository<MovimientosFinancierosEntity, int> _movimientosRepository = movimientosRepository;

        public async Task CrearMovimientoAsync(CrearMovimientoFinancieroDTO dto, Guid idUsuario)
        {
            var entity = new MovimientosFinancierosEntity
            {
                TipoMovimiento = dto.TipoMovimiento,
                SubTipo = dto.SubTipo,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                Direccion = dto.Direccion,
                OrigenModulo = dto.OrigenModulo,
                ReferenciaId = dto.ReferenciaId,
                FechaMovimiento = dto.FechaMovimiento ?? DateTime.Now,
                Estado = dto.Estado ?? 1,
                FechaCreacion = DateTime.Now,
                IdUsuario = idUsuario == Guid.Empty ? null : idUsuario
            };
            await _movimientosRepository.CreateAsync(entity);
        }

        public async Task<List<MovimientosFinancierosEntity>> ObtenerMovimientosAsync(int? estado = null)
        {
            if (estado.HasValue)
                return _movimientosRepository.GetByFilter(m => m.Estado == estado.Value);
            return await _movimientosRepository.GetAllAsync();
        }

        public async Task<MovimientosFinancierosEntity> ObtenerMovimientoPorIdAsync(int id)
        {
            var entity = await _movimientosRepository.FindByIdAsync(id);
            return entity ?? throw new Exception("El movimiento financiero no existe");
        }

        public async Task ActualizarMovimientoAsync(int id, CrearMovimientoFinancieroDTO dto, Guid idUsuario)
        {
            var entity = await _movimientosRepository.FindByIdAsync(id);
            if (entity == null)
            {
                throw new Exception("El movimiento financiero no existe");
            }

            entity.TipoMovimiento = dto.TipoMovimiento;
            entity.SubTipo = dto.SubTipo;
            entity.Descripcion = dto.Descripcion;
            entity.Monto = dto.Monto;
            entity.Direccion = dto.Direccion;
            entity.OrigenModulo = dto.OrigenModulo;
            entity.ReferenciaId = dto.ReferenciaId;
            entity.FechaMovimiento = dto.FechaMovimiento ?? entity.FechaMovimiento;
            entity.Estado = dto.Estado ?? entity.Estado;
            entity.FechaActualizacion = DateTime.Now;
            entity.IdUsuario = idUsuario == Guid.Empty ? entity.IdUsuario : idUsuario;

            await _movimientosRepository.UpdateAsync(entity);
        }

        public async Task EliminarMovimientoAsync(int id)
        {
            var existente = await _movimientosRepository.FindByIdAsync(id);
            if (existente == null)
            {
                throw new Exception("El movimiento financiero no existe");
            }

            await _movimientosRepository.DeleteAsync(id);
        }
    }
}
