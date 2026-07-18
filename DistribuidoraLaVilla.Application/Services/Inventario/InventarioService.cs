using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Domain.DTOS.Inventario;
using DistribuidoraLaVilla.Domain.DTOS.Productos;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services.Inventario
{
    /// <summary>
    /// Implementación compartida de inventario FIFO.
    /// Centraliza la lógica de consumo de lotes para que Producción,
    /// Facturación y otros módulos usen el mismo algoritmo.
    /// </summary>
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository;
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosMateriaPrimaRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;

        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository;
        private readonly IGenericRepository<MovimientoEntity, int> _movimientosRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;

        public InventarioService(
            IUnitOfWork unitOfWork,
            IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository,
            IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosMateriaPrimaRepository,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
            IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
            IGenericRepository<MovimientoEntity, int> movimientosRepository,
            IGenericRepository<ProductosEntity, int> productosRepository)
        {
            _unitOfWork = unitOfWork;
            _lotesMateriaPrimaRepository = lotesMateriaPrimaRepository;
            _movimientosMateriaPrimaRepository = movimientosMateriaPrimaRepository;
            _materiaPrimaRepository = materiaPrimaRepository;
            _unidadMedidaRepository = unidadMedidaRepository;

            _lotesProductosRepository = lotesProductosRepository;
            _movimientosRepository = movimientosRepository;
            _productosRepository = productosRepository;
        }

        /// <inheritdoc/>
        public async Task<List<ConsumoIngredienteDTO>> ConsumirLotesMateriaPrimaAsync(
            int idMateriaPrima,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            string observacion)
        {
            bool ownTransaction = !_unitOfWork.HasActiveTransaction;

            if (ownTransaction)
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(idMateriaPrima)
                    ?? throw new InvalidOperationException(
                        $"No se encontró la materia prima con ID {idMateriaPrima}");

                var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

                var lotesDisponibles = _lotesMateriaPrimaRepository.GetByFilter(l =>
                    l.IdMateria == idMateriaPrima &&
                    l.Estado == 1 &&
                    l.CantidadDisponible > 0
                ).OrderBy(l => l.FechaVencimiento).ToList();

                var stockTotal = lotesDisponibles.Sum(l => l.CantidadDisponible);

                if (stockTotal < cantidadRequerida)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente de '{materiaPrima.Nombre}'. " +
                        $"Requerido: {cantidadRequerida}, Disponible: {stockTotal}");
                }

                var consumos = new List<ConsumoIngredienteDTO>();
                decimal cantidadPendiente = cantidadRequerida;

                foreach (var lote in lotesDisponibles)
                {
                    if (cantidadPendiente <= 0) break;

                    decimal cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);

                    lote.CantidadDisponible -= cantidadAConsumir;
                    await _lotesMateriaPrimaRepository.UpdateAsync(lote);

                    var movimiento = new MovimientosMateriaPrimaEntity
                    {
                        IdLoteMateria = lote.Id,
                        IdTipoMovimiento = (int)TipoMovimientoMateriaPrima.Consumo,
                        Fecha = DateTime.Now,
                        Cantidad = cantidadAConsumir,
                        IdUnidadMedida = idUnidadMedida,
                        IdUsuario = idUsuario,
                        Observacion = observacion
                    };

                    await _movimientosMateriaPrimaRepository.CreateAsync(movimiento);

                    consumos.Add(new ConsumoIngredienteDTO
                    {
                        IdMateriaPrima = idMateriaPrima,
                        NombreMateriaPrima = materiaPrima.Nombre,
                        CantidadRequerida = cantidadRequerida,
                        CantidadConsumida = cantidadAConsumir,
                        UnidadMedida = unidadMedida?.Abreviatura,
                        IdMovimiento = movimiento.Id
                    });

                    cantidadPendiente -= cantidadAConsumir;
                }

                if (ownTransaction)
                    await _unitOfWork.CommitAsync();

                return consumos;
            }
            catch
            {
                if (ownTransaction)
                    await _unitOfWork.RollbackAsync();
                throw;
            }
            finally
            {
                if (ownTransaction)
                    await _unitOfWork.DisposeAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<List<ConsumoProductoDTO>> ConsumirLotesProductoAsync(
            int idProducto,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            string observacion,
            bool esVentaPorPeso,
            decimal? pesoPorUnidad = null)
        {
            var producto = await _productosRepository.FindByIdAsync(idProducto)
                ?? throw new InvalidOperationException(
                    $"No se encontró el producto con ID {idProducto}");

            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

            var lotesDisponibles = _lotesProductosRepository.GetByFilter(l =>
                l.IdProducto == idProducto &&
                l.Estado == 1 &&
                l.CantidadDisponible > 0
            ).OrderBy(l => l.FechaVencimiento).ToList();

            if (pesoPorUnidad.HasValue)
            {
                if (esVentaPorPeso)
                {
                    var pesoTotal = lotesDisponibles.Sum(l => l.PesoDisponible);
                    if (pesoTotal < cantidadRequerida)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente de '{producto.Nombre}'. " +
                            $"Requerido: {cantidadRequerida} kg, Disponible: {pesoTotal} kg");
                    }
                }
                else
                {
                    var stockTotal = lotesDisponibles.Sum(l => l.CantidadDisponible);
                    if (stockTotal < cantidadRequerida)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente de '{producto.Nombre}'. " +
                            $"Requerido: {cantidadRequerida} unidades, Disponible: {stockTotal} unidades");
                    }
                }
            }
            else
            {
                var stockTotal = lotesDisponibles.Sum(l => l.CantidadDisponible);
                if (stockTotal < cantidadRequerida)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente de '{producto.Nombre}'. " +
                        $"Requerido: {cantidadRequerida}, Disponible: {stockTotal}");
                }
            }

            var consumos = new List<ConsumoProductoDTO>();
            decimal cantidadPendiente = cantidadRequerida;

            foreach (var lote in lotesDisponibles)
            {
                if (cantidadPendiente <= 0) break;

                decimal cantidadAConsumir;
                decimal unidadesAConsumir = 0;
                decimal pesoAConsumir = 0;

                if (pesoPorUnidad.HasValue && esVentaPorPeso)
                {
                    cantidadAConsumir = Math.Min(lote.PesoDisponible, cantidadPendiente);
                    unidadesAConsumir = cantidadAConsumir / pesoPorUnidad.Value;
                    pesoAConsumir = cantidadAConsumir;

                    lote.PesoDisponible -= pesoAConsumir;
                    lote.CantidadDisponible -= unidadesAConsumir;
                }
                else if (pesoPorUnidad.HasValue && !esVentaPorPeso)
                {
                    cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);
                    unidadesAConsumir = cantidadAConsumir;
                    pesoAConsumir = cantidadAConsumir * pesoPorUnidad.Value;

                    lote.CantidadDisponible -= unidadesAConsumir;
                    lote.PesoDisponible -= pesoAConsumir;
                }
                else
                {
                    cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);
                    lote.CantidadDisponible -= cantidadAConsumir;
                }

                await _lotesProductosRepository.UpdateAsync(lote);

                var costBasis = esVentaPorPeso
                    ? lote.PrecioKilo
                    : lote.PrecioUnitario;

                if (esVentaPorPeso && lote.PrecioKilo <= 0)
                {
                    throw new InvalidOperationException(
                        $"El lote del producto '{producto.Nombre}' tiene PrecioKilo inválido ({lote.PrecioKilo}) para una venta por peso");
                }

                var movimiento = new MovimientoEntity
                {
                    IdLoteProducto = lote.Id,
                    TipoMovimiento = (int)TipoMovimientoProducto.Venta,
                    FechaMovimiento = DateTime.Now,
                    Cantidad = cantidadAConsumir,
                    TotalMovimiento = cantidadAConsumir * costBasis,
                    IdUnidadMedida = idUnidadMedida,
                    IdEntidad = null,
                    IdUsuario = idUsuario,
                    Observacion = observacion,
                    Estado = 1
                };

                await _movimientosRepository.CreateAsync(movimiento);

                consumos.Add(new ConsumoProductoDTO
                {
                    IdProducto = idProducto,
                    NombreProducto = producto.Nombre,
                    CantidadRequerida = cantidadRequerida,
                    CantidadConsumida = cantidadAConsumir,
                    UnidadMedida = unidadMedida?.Abreviatura,
                    IdMovimiento = movimiento.Id,
                    IdLote = lote.Id
                });

                cantidadPendiente -= cantidadAConsumir;
            }

            return consumos;
        }
    }
}
