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
        private readonly IGenericRepository<LotesMateriaPrimaEntity, int> _lotesMateriaPrimaRepository;
        private readonly IGenericRepository<MovimientosMateriaPrimaEntity, int> _movimientosMateriaPrimaRepository;
        private readonly IGenericRepository<MateriaPrimaEntity, int> _materiaPrimaRepository;
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedidaRepository;

        // Repositorios para productos terminados
        private readonly IGenericRepository<LotesProductosEntity, int> _lotesProductosRepository;
        private readonly IGenericRepository<MovimientoEntity, int> _movimientosRepository;
        private readonly IGenericRepository<ProductosEntity, int> _productosRepository;

        public InventarioService(
            IGenericRepository<LotesMateriaPrimaEntity, int> lotesMateriaPrimaRepository,
            IGenericRepository<MovimientosMateriaPrimaEntity, int> movimientosMateriaPrimaRepository,
            IGenericRepository<MateriaPrimaEntity, int> materiaPrimaRepository,
            IGenericRepository<UnidadMedidaEntity, int> unidadMedidaRepository,
            IGenericRepository<LotesProductosEntity, int> lotesProductosRepository,
            IGenericRepository<MovimientoEntity, int> movimientosRepository,
            IGenericRepository<ProductosEntity, int> productosRepository)
        {
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
            var materiaPrima = await _materiaPrimaRepository.FindByIdAsync(idMateriaPrima)
                ?? throw new InvalidOperationException(
                    $"No se encontró la materia prima con ID {idMateriaPrima}");

            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

            // Obtener lotes disponibles ordenados por vencimiento (FIFO)
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

            // Consumir de los lotes ordenados por vencimiento (FIFO)
            foreach (var lote in lotesDisponibles)
            {
                if (cantidadPendiente <= 0) break;

                decimal cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);

                // Descontar del lote
                lote.CantidadDisponible -= cantidadAConsumir;
                await _lotesMateriaPrimaRepository.UpdateAsync(lote);

                // Crear movimiento de consumo
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

            return consumos;
        }

        /// <inheritdoc/>
        public async Task<List<ConsumoProductoDTO>> ConsumirLotesProductoAsync(
            int idProducto,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            string observacion)
        {
            // 1. Verificar que el producto exista
            var producto = await _productosRepository.FindByIdAsync(idProducto)
                ?? throw new InvalidOperationException(
                    $"No se encontró el producto con ID {idProducto}");

            var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

            // 2. Obtener lotes disponibles ordenados por vencimiento (FIFO)
            var lotesDisponibles = _lotesProductosRepository.GetByFilter(l =>
                l.IdProducto == idProducto &&
                l.Estado == 1 &&
                l.CantidadDisponible > 0
            ).OrderBy(l => l.FechaVencimiento).ToList();

            // 3. Validar stock suficiente
            var stockTotal = lotesDisponibles.Sum(l => l.CantidadDisponible);

            if (stockTotal < cantidadRequerida)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente de '{producto.Nombre}'. " +
                    $"Requerido: {cantidadRequerida}, Disponible: {stockTotal}");
            }

            var consumos = new List<ConsumoProductoDTO>();
            decimal cantidadPendiente = cantidadRequerida;

            // Consumir de los lotes ordenados por vencimiento (FIFO)
            foreach (var lote in lotesDisponibles)
            {
                if (cantidadPendiente <= 0) break;

                decimal cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);

                // 4. Descontar CantidadDisponible del lote
                lote.CantidadDisponible -= cantidadAConsumir;
                await _lotesProductosRepository.UpdateAsync(lote);

                // 5. Crear registro en movimientos (tipo_movimiento = 2 Venta)
                // 6. Calcular total_movimiento = cantidad * PrecioKilo (weight) or PrecioUnitario (unit)
                var costBasis = producto.VentaPorPeso
                    ? lote.PrecioKilo
                    : lote.PrecioUnitario;

                // BR-VP-06: Validate PrecioKilo > 0 for weight products
                if (producto.VentaPorPeso && lote.PrecioKilo <= 0)
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
                    IdEntidad = null, // se asigna en FacturaService si se quiere vincular al cliente
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

            // 9. Retornar lista de ConsumoProductoDTO
            return consumos;
        }
    }
}
