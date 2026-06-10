using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class MovimientosMateriaPrimaService(
        IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosRepository,
        IGenericRepository<LotesMateriaPrimaEntity, int> lotesRepository,
        IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
        IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
        IGenericRepository<MarcasEntity, int> marcasRepository,
        IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository)
    {
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosRepository = movimientosRepository;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesRepository = lotesRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository = materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository = unidadMedidaRepository;
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;

        public async Task<List<MovimientosMateriaPrimaEntity>> ObtenerMovimientosAsync()
        {
            return await _movimientosRepository.GetAllAsync();
        }

        public async Task<List<MovimientoMateriaPrimaListDTO>> ObtenerMovimientosDetalleAsync()
        {
            var movimientos = await _movimientosRepository.GetAllAsync();
            var lotes = (await _lotesRepository.GetAllAsync()).ToDictionary(l => l.Id);
            var materias = (await _materiaPrimaRepository.GetAllAsync()).ToDictionary(m => m.Id);
            var marcas = (await _marcasRepository.GetAllAsync()).ToDictionary(m => m.IdMarca);
            var proveedores = (await _proveedoresRepository.GetAllAsync()).ToDictionary(p => p.IdProveedor);
            var unidades = (await _unidadMedidaRepository.GetAllAsync()).ToDictionary(u => u.Id);

            return movimientos.Select(m =>
            {
                string? nombreLote = null;
                if (lotes.TryGetValue(m.IdLoteMateria, out var lote))
                {
                    var nombreMateria = materias.ContainsKey(lote.IdMateria) ? materias[lote.IdMateria].Nombre : "";
                    var nombreMarca = marcas.ContainsKey(lote.IdMarca) ? marcas[lote.IdMarca].Nombre : "";
                    nombreLote = $"{nombreMateria} - {nombreMarca}";
                }

                return new MovimientoMateriaPrimaListDTO
                {
                    Id = m.Id,
                    IdLoteMateria = m.IdLoteMateria,
                    NombreLote = nombreLote,
                    IdTipoMovimiento = m.IdTipoMovimiento,
                    TipoMovimientoNombre = ObtenerNombreTipoMovimiento(m.IdTipoMovimiento),
                    Fecha = m.Fecha,
                    Cantidad = m.Cantidad,
                    IdUnidadMedida = m.IdUnidadMedida,
                    NombreUnidadMedida = unidades.ContainsKey(m.IdUnidadMedida) ? unidades[m.IdUnidadMedida].Nombre : "N/A",
                    SimboloUnidadMedida = unidades.ContainsKey(m.IdUnidadMedida) ? unidades[m.IdUnidadMedida].Abreviatura : "N/A",
                    IdUsuario = m.IdUsuario,
                    Observacion = m.Observacion,
                    StockAnterior = 0,
                    StockNuevo = 0
                };
            }).OrderByDescending(m => m.Fecha).ToList();
        }

        public async Task<MovimientosMateriaPrimaEntity> ObtenerMovimientoPorIdAsync(int id)
        {
            var movimiento = await _movimientosRepository.FindByIdAsync(id);
            return movimiento ?? throw new Exception($"No se encontró el movimiento con ID {id}");
        }

        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorLoteAsync(int idLote)
        {
            return _movimientosRepository.GetByFilter(m => m.IdLoteMateria == idLote);
        }

        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorTipoAsync(int idTipoMovimiento)
        {
            return _movimientosRepository.GetByFilter(m => m.IdTipoMovimiento == idTipoMovimiento);
        }

        public List<MovimientosMateriaPrimaEntity> ObtenerMovimientosPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return _movimientosRepository.GetByFilter(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
        }

        private static string ObtenerNombreTipoMovimiento(int tipoMovimiento)
        {
            return tipoMovimiento switch
            {
                (int)TipoMovimientoMateriaPrima.Entrada => "Entrada",
                (int)TipoMovimientoMateriaPrima.Consumo => "Consumo",
                (int)TipoMovimientoMateriaPrima.Ajuste => "Ajuste",
                (int)TipoMovimientoMateriaPrima.Devolucion => "Devolución",
                (int)TipoMovimientoMateriaPrima.Vencimiento => "Vencimiento",
                _ => "Desconocido"
            };
        }
    }
}
