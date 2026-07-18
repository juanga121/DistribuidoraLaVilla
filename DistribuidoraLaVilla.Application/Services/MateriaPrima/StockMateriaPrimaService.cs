using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services.MateriaPrima
{
    public class StockMateriaPrimaService
    {
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;

        public StockMateriaPrimaService(
            IGenericRepository<LotesMateriaPrimaEntity, int> lotesRepository,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository)
        {
            _lotesRepository = lotesRepository;
            _materiaPrimaRepository = materiaPrimaRepository;
            _unidadMedidaRepository = unidadMedidaRepository;
        }

        /// <summary>
        /// Obtiene el stock consolidado de todas las materias primas
        /// </summary>
        public async Task<List<StockMateriaPrimaDTO>> ObtenerStockConsolidadoAsync()
        {
            var lotesActivos = _lotesRepository.GetByFilter(l => 
                l.Estado == 1 && 
                l.CantidadDisponible > 0
            );

            if (!lotesActivos.Any())
            {
                return new List<StockMateriaPrimaDTO>();
            }

            var materiasPrimas = await _materiaPrimaRepository.GetAllAsync();
            var materiasPrimasDict = materiasPrimas
                .Where(mp => mp.Estado == 1)
                .ToDictionary(mp => mp.Id, mp => mp);

            var unidadesMedida = await _unidadMedidaRepository.GetAllAsync();
            var unidadesMedidaDict = unidadesMedida.ToDictionary(um => um.Id, um => um);

            var stockPorMateria = lotesActivos
                .Where(l => materiasPrimasDict.ContainsKey(l.IdMateria))
                .GroupBy(l => l.IdMateria)
                .Select(grupo => new StockMateriaPrimaDTO
                {
                    IdMateriaPrima = grupo.Key,
                    NombreMateriaPrima = materiasPrimasDict[grupo.Key].Nombre,
                    StockDisponible = grupo.Sum(l => l.CantidadDisponible),
                    IdUnidadMedida = grupo.First().IdUnidadMedida,
                    NombreUnidadMedida = unidadesMedidaDict.ContainsKey(grupo.First().IdUnidadMedida) 
                        ? unidadesMedidaDict[grupo.First().IdUnidadMedida].Nombre 
                        : "N/A",
                    SimboloUnidadMedida = unidadesMedidaDict.ContainsKey(grupo.First().IdUnidadMedida) 
                        ? unidadesMedidaDict[grupo.First().IdUnidadMedida].Abreviatura 
                        : "N/A",
                    LotesDisponibles = grupo.Count(),
                    ProximaFechaVencimiento = grupo
                        .OrderBy(l => l.FechaVencimiento)
                        .First()
                        .FechaVencimiento,
                    CostoPromedio = grupo.Sum(l => l.CantidadDisponible) > 0 
                        ? grupo.Sum(l => l.CostoUnitario * l.CantidadDisponible) / grupo.Sum(l => l.CantidadDisponible)
                        : 0,
                    ValorTotalStock = grupo.Sum(l => l.CantidadDisponible * l.CostoUnitario),
                    DetalleLotes = grupo
                        .OrderBy(l => l.FechaVencimiento)
                        .Select(l => new LoteStockDTO
                        {
                            IdLote = l.Id,
                            CantidadDisponible = l.CantidadDisponible,
                            FechaVencimiento = l.FechaVencimiento,
                            FechaEntrada = l.FechaEntrada,
                            NombreMarca = "",
                            NombreProveedor = "",
                            CostoUnitario = l.CostoUnitario,
                            CostoTotalLote = l.CostoTotal,
                            DiasParaVencimiento = (int)(l.FechaVencimiento - DateTime.Now).TotalDays
                        })
                        .ToList()
                })
                .OrderBy(s => s.ProximaFechaVencimiento)
                .ToList();

            return stockPorMateria;
        }

        /// <summary>
        /// Obtiene el stock de una materia prima específica por ID
        /// </summary>
        public async Task<StockMateriaPrimaDTO?> ObtenerStockPorIdAsync(int idMateriaPrima)
        {
            var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(idMateriaPrima);
            if (materiaPrima == null || materiaPrima.Estado != 1)
            {
                return null;
            }

            var lotesActivos = _lotesRepository.GetByFilter(l => 
                l.IdMateria == idMateriaPrima && 
                l.Estado == 1 && 
                l.CantidadDisponible > 0
            );

            if (!lotesActivos.Any())
            {
                var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(1);
                return new StockMateriaPrimaDTO
                {
                    IdMateriaPrima = materiaPrima.Id,
                    NombreMateriaPrima = materiaPrima.Nombre,
                    StockDisponible = 0,
                    IdUnidadMedida = 1,
                    NombreUnidadMedida = unidadMedida?.Nombre ?? "N/A",
                    SimboloUnidadMedida = unidadMedida?.Abreviatura ?? "N/A",
                    LotesDisponibles = 0,
                    ProximaFechaVencimiento = null,
                    DetalleLotes = new List<LoteStockDTO>()
                };
            }

            var idUnidadMedida = lotesActivos.First().IdUnidadMedida;
            var unidad = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

            var stock = new StockMateriaPrimaDTO
            {
                IdMateriaPrima = materiaPrima.Id,
                NombreMateriaPrima = materiaPrima.Nombre,
                StockDisponible = lotesActivos.Sum(l => l.CantidadDisponible),
                IdUnidadMedida = idUnidadMedida,
                NombreUnidadMedida = unidad?.Nombre ?? "N/A",
                SimboloUnidadMedida = unidad?.Abreviatura ?? "N/A",
                LotesDisponibles = lotesActivos.Count,
                ProximaFechaVencimiento = lotesActivos
                    .OrderBy(l => l.FechaVencimiento)
                    .First()
                    .FechaVencimiento,
                CostoPromedio = lotesActivos.Sum(l => l.CantidadDisponible) > 0
                    ? lotesActivos.Sum(l => l.CostoUnitario * l.CantidadDisponible) / lotesActivos.Sum(l => l.CantidadDisponible)
                    : 0,
                ValorTotalStock = lotesActivos.Sum(l => l.CantidadDisponible * l.CostoUnitario),
                DetalleLotes = lotesActivos
                    .OrderBy(l => l.FechaVencimiento)
                    .Select(l => new LoteStockDTO
                    {
                        IdLote = l.Id,
                        CantidadDisponible = l.CantidadDisponible,
                        FechaVencimiento = l.FechaVencimiento,
                        FechaEntrada = l.FechaEntrada,
                        NombreMarca = "",
                        NombreProveedor = "",
                        CostoUnitario = l.CostoUnitario,
                        CostoTotalLote = l.CostoTotal,
                        DiasParaVencimiento = (int)(l.FechaVencimiento - DateTime.Now).TotalDays
                    })
                    .ToList()
            };

            return stock;
        }

        /// <summary>
        /// Obtiene materias primas con stock bajo (menos del mínimo especificado)
        /// </summary>
        public async Task<List<StockMateriaPrimaDTO>> ObtenerStockBajoAsync(decimal cantidadMinima = 10)
        {
            var stockCompleto = await ObtenerStockConsolidadoAsync();
            return stockCompleto
                .Where(s => s.StockDisponible > 0 && s.StockDisponible < cantidadMinima)
                .OrderBy(s => s.StockDisponible)
                .ToList();
        }

        /// <summary>
        /// Obtiene materias primas sin stock
        /// </summary>
        public async Task<List<StockMateriaPrimaDTO>> ObtenerSinStockAsync()
        {
            var todasLasMateriasPrimas = await _materiaPrimaRepository.GetAllAsync();
            var materiasActivas = todasLasMateriasPrimas.Where(mp => mp.Estado == 1).ToList();

            var stockCompleto = await ObtenerStockConsolidadoAsync();
            var idsConStock = stockCompleto.Select(s => s.IdMateriaPrima).ToHashSet();

            var unidadDefault = await _unidadMedidaRepository.FindByIdAsync(1);

            var sinStock = materiasActivas
                .Where(mp => !idsConStock.Contains(mp.Id))
                .Select(mp => new StockMateriaPrimaDTO
                {
                    IdMateriaPrima = mp.Id,
                    NombreMateriaPrima = mp.Nombre,
                    StockDisponible = 0,
                    IdUnidadMedida = 1,
                    NombreUnidadMedida = unidadDefault?.Nombre ?? "N/A",
                    SimboloUnidadMedida = unidadDefault?.Abreviatura ?? "N/A",
                    LotesDisponibles = 0,
                    ProximaFechaVencimiento = null,
                    DetalleLotes = new List<LoteStockDTO>()
                })
                .ToList();

            return sinStock;
        }

        /// <summary>
        /// Obtiene lotes próximos a vencer (dentro de los días especificados)
        /// </summary>
        public async Task<List<StockMateriaPrimaDTO>> ObtenerProximosAVencerAsync(int diasAnticipacion = 30)
        {
            var fechaLimite = DateTime.Now.AddDays(diasAnticipacion);
            var stockCompleto = await ObtenerStockConsolidadoAsync();

            return stockCompleto
                .Where(s => s.ProximaFechaVencimiento.HasValue && 
                           s.ProximaFechaVencimiento.Value <= fechaLimite)
                .OrderBy(s => s.ProximaFechaVencimiento)
                .ToList();
        }
    }
}
