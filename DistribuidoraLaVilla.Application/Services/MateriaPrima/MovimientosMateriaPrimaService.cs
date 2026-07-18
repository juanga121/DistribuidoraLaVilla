using System.Text.Json;
using DistribuidoraLaVilla.Application.Common;
using DistribuidoraLaVilla.Application.Interfaces;
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
        IGenericRepository<ProveedoresEntity, Guid> proveedoresRepository,
        IGenericRepository<TipoMovimientoMateriaPrimaEntity, int> tipoMovimientoRepository,
        IAuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosRepository = movimientosRepository;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesRepository = lotesRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository = materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository = unidadMedidaRepository;
        private readonly IGenericRepository<MarcasEntity, int> _marcasRepository = marcasRepository;
        private readonly IGenericRepository<ProveedoresEntity, Guid> _proveedoresRepository = proveedoresRepository;
        private readonly IGenericRepository<TipoMovimientoMateriaPrimaEntity, int> _tipoMovimientoRepository = tipoMovimientoRepository;
        private readonly IAuditoriaService _auditoriaService = auditoriaService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

            var stockCalculado = CalcularStockMovimientos(movimientos);

            return movimientos.Select(m =>
            {
                string? nombreLote = null;
                if (lotes.TryGetValue(m.IdLoteMateria, out var lote))
                {
                    var nombreMateria = materias.ContainsKey(lote.IdMateria) ? materias[lote.IdMateria].Nombre : "";
                    var nombreMarca = marcas.ContainsKey(lote.IdMarca) ? marcas[lote.IdMarca].Nombre : "";
                    nombreLote = $"{nombreMateria} - {nombreMarca}";
                }

                var (anterior, nuevo) = stockCalculado.TryGetValue(m.Id, out var sv) ? sv : (0m, 0m);

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
                    StockAnterior = anterior,
                    StockNuevo = nuevo
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

        /// <summary>
        /// Crea un movimiento manual de materia prima con validaciones y actualización de stock del lote.
        /// Todo dentro de una transacción atómica.
        /// </summary>
        public async Task<ApiResponse<MovimientoMateriaPrimaResponseDTO>> CrearMovimientoMateriaPrimaAsync(MovimientoMateriaPrimaDTO dto)
        {
            var lote = await _lotesRepository.FindByIdAsync(dto.IdLoteMateria);
            if (lote == null)
                return ApiResponse<MovimientoMateriaPrimaResponseDTO>.Fail("El lote de materia prima no existe");

            var tipoMovimiento = await _tipoMovimientoRepository.FindByIdAsync(dto.IdTipoMovimiento);
            if (tipoMovimiento == null)
                return ApiResponse<MovimientoMateriaPrimaResponseDTO>.Fail("El tipo de movimiento no existe");

            if (dto.Cantidad <= 0)
                return ApiResponse<MovimientoMateriaPrimaResponseDTO>.Fail("La cantidad debe ser mayor a 0");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Consumo ||
                    dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Vencimiento)
                {
                    if (lote.CantidadDisponible < dto.Cantidad)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ApiResponse<MovimientoMateriaPrimaResponseDTO>.Fail(
                            $"Stock insuficiente. Disponible: {lote.CantidadDisponible}, requerido: {dto.Cantidad}");
                    }

                    lote.CantidadDisponible -= dto.Cantidad;
                    await _lotesRepository.UpdateAsync(lote);
                }

                if (dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Entrada ||
                    dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Devolucion)
                {
                    lote.CantidadDisponible += dto.Cantidad;
                    lote.Cantidad += dto.Cantidad;
                    await _lotesRepository.UpdateAsync(lote);
                }

                var movimiento = new MovimientosMateriaPrimaEntity
                {
                    IdLoteMateria = dto.IdLoteMateria,
                    IdTipoMovimiento = dto.IdTipoMovimiento,
                    Fecha = DateTime.Now,
                    Cantidad = dto.Cantidad,
                    IdUnidadMedida = dto.IdUnidadMedida,
                    IdUsuario = dto.IdUsuario,
                    Observacion = dto.Observacion
                };

                await _movimientosRepository.CreateAsync(movimiento);

                await _unitOfWork.CommitAsync();

                await RegistrarAuditoriaAsync("StockMP", movimiento.Id.ToString(), "CrearMovimiento", new
                {
                    idLoteMateria = dto.IdLoteMateria,
                    idTipoMovimiento = dto.IdTipoMovimiento,
                    cantidad = dto.Cantidad,
                    idUnidadMedida = dto.IdUnidadMedida
                }, dto.IdUsuario);

                var responseDto = new MovimientoMateriaPrimaResponseDTO
                {
                    Id = movimiento.Id,
                    IdLoteMateria = movimiento.IdLoteMateria,
                    IdTipoMovimiento = movimiento.IdTipoMovimiento,
                    TipoMovimientoNombre = ObtenerNombreTipoMovimiento(movimiento.IdTipoMovimiento),
                    Fecha = movimiento.Fecha,
                    Cantidad = movimiento.Cantidad,
                    IdUnidadMedida = movimiento.IdUnidadMedida,
                    IdUsuario = movimiento.IdUsuario,
                    Observacion = movimiento.Observacion,
                    StockAnterior = dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Consumo ||
                                    dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Vencimiento
                        ? lote.CantidadDisponible + dto.Cantidad  // antes de descontar
                        : (dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Entrada ||
                           dto.IdTipoMovimiento == (int)TipoMovimientoMateriaPrima.Devolucion
                            ? lote.CantidadDisponible - dto.Cantidad  // antes de sumar
                            : lote.CantidadDisponible),
                    StockNuevo = lote.CantidadDisponible
                };

                return ApiResponse<MovimientoMateriaPrimaResponseDTO>.Ok(responseDto, "Movimiento creado correctamente");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            finally
            {
                await _unitOfWork.DisposeAsync();
            }
        }

        /// <summary>
        /// Calcula StockAnterior y StockNuevo para cada movimiento,
        /// agrupando por lote y ordenando por fecha (y Id como desempate).
        /// Los resultados se devuelven en un diccionario { id_movimiento => (anterior, nuevo) }.
        /// </summary>
        private static Dictionary<int, (decimal anterior, decimal nuevo)> CalcularStockMovimientos(
            List<MovimientosMateriaPrimaEntity> movimientos)
        {
            var resultado = new Dictionary<int, (decimal anterior, decimal nuevo)>();

            var movimientosPorLote = movimientos
                .GroupBy(m => m.IdLoteMateria)
                .SelectMany(g => g.OrderBy(m => m.Fecha).ThenBy(m => m.Id))
                .ToList();

            var stockActual = new Dictionary<int, decimal>();

            foreach (var m in movimientosPorLote)
            {
                if (!stockActual.ContainsKey(m.IdLoteMateria))
                    stockActual[m.IdLoteMateria] = 0;

                var anterior = stockActual[m.IdLoteMateria];
                var delta = ObtenerCantidadConSigno(m);
                var nuevo = anterior + delta;

                resultado[m.Id] = (anterior, nuevo);
                stockActual[m.IdLoteMateria] = nuevo;
            }

            return resultado;
        }

        /// <summary>
        /// Devuelve la cantidad con signo según el tipo de movimiento:
        /// Entrada/Devolución → positivo; Consumo/Vencimiento → negativo; Ajuste → según el signo almacenado.
        /// </summary>
        private static decimal ObtenerCantidadConSigno(MovimientosMateriaPrimaEntity m)
        {
            return m.IdTipoMovimiento switch
            {
                (int)TipoMovimientoMateriaPrima.Entrada => m.Cantidad,
                (int)TipoMovimientoMateriaPrima.Consumo => -m.Cantidad,
                (int)TipoMovimientoMateriaPrima.Devolucion => m.Cantidad,
                (int)TipoMovimientoMateriaPrima.Vencimiento => -m.Cantidad,
                _ => m.Cantidad
            };
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
