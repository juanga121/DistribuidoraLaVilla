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

                var hoy = DateTime.Now.Date;
                var lotesDisponibles = _lotesMateriaPrimaRepository.GetByFilter(l =>
                    l.IdMateria == idMateriaPrima &&
                    l.Estado == 1 &&
                    l.CantidadDisponible > 0 &&
                    l.FechaVencimiento >= hoy
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
                decimal costoTotalConsumido = 0m;

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

                    costoTotalConsumido += cantidadAConsumir * lote.CostoUnitario;

                    consumos.Add(new ConsumoIngredienteDTO
                    {
                        IdMateriaPrima = idMateriaPrima,
                        NombreMateriaPrima = materiaPrima.Nombre,
                        CantidadRequerida = cantidadRequerida,
                        CantidadConsumida = cantidadAConsumir,
                        UnidadMedida = unidadMedida?.Abreviatura,
                        IdMovimiento = movimiento.Id,
                        CostoTotalConsumido = cantidadAConsumir * lote.CostoUnitario
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
            Guid idCliente,
            string observacion,
            bool esVentaPorPeso,
            decimal? pesoRealLinea = null)
        {
            bool ownTransaction = !_unitOfWork.HasActiveTransaction;

            if (ownTransaction)
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                var producto = await _productosRepository.FindByIdAsync(idProducto)
                    ?? throw new InvalidOperationException(
                        $"No se encontró el producto con ID {idProducto}");

                // The product's inventory mode owns the meaning of the quantity: weight modes keep
                // their stock in kilograms and can only be sold by weight, unit mode sells by unit.
                // The mixed mode keeps stock in kilograms but accepts both, converting a unit sale
                // with the product's fixed weight per unit.
                var operaPorPeso = ProductoStockRules.OperaPorPeso(producto);
                var permiteMixta = ProductoStockRules.PermiteVentaMixta(producto);

                if (!permiteMixta && operaPorPeso && !esVentaPorPeso)
                {
                    throw new InvalidOperationException(
                        $"El producto '{producto.Nombre}' se administra por peso: la venta debe informarse en kilogramos");
                }

                if (!permiteMixta && !operaPorPeso && esVentaPorPeso)
                {
                    throw new InvalidOperationException(
                        $"El producto '{producto.Nombre}' se administra por unidad: la venta debe informarse en unidades");
                }

                // En el modo mixto, una venta por unidad mueve el stock con el peso real pesado en
                // el mostrador. No hay conversión: el factor sale de esta venta, no del producto.
                var factorAKg = 1m;
                if (permiteMixta && !esVentaPorPeso)
                {
                    if (!pesoRealLinea.HasValue || pesoRealLinea.Value <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Para vender '{producto.Nombre}' por unidad hay que informar el peso de la mercadería: pesá las unidades y cargá el total en kg");
                    }

                    if (cantidadRequerida <= 0)
                    {
                        throw new InvalidOperationException(
                            $"La cantidad vendida de '{producto.Nombre}' debe ser mayor a 0");
                    }

                    factorAKg = pesoRealLinea.Value / cantidadRequerida;
                }

                // Cantidad a descontar del stock, siempre en kilogramos.
                var cantidadRequeridaEnStock = cantidadRequerida * factorAKg;

                var unidadMedida = await _unidadMedidaRepository.FindByIdAsync(idUnidadMedida);

                var hoy = DateTime.Now.Date;
                // Los lotes por peso pueden tener CantidadDisponible == 0 y aun así conservar
                // kilogramos disponibles, por eso el filtro depende del modo de inventario.
                var lotesDisponibles = _lotesProductosRepository.GetByFilter(l =>
                    l.IdProducto == idProducto &&
                    l.Estado == 1 &&
                    (operaPorPeso ? l.PesoDisponible > 0 : l.CantidadDisponible > 0) &&
                    l.FechaVencimiento >= hoy
                ).OrderBy(l => l.FechaVencimiento).ToList();

                if (operaPorPeso)
                {
                    var pesoTotal = lotesDisponibles.Sum(l => l.PesoDisponible);
                    if (pesoTotal < cantidadRequeridaEnStock)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente de '{producto.Nombre}'. " +
                            $"Requerido: {cantidadRequeridaEnStock} kg, Disponible: {pesoTotal} kg");
                    }
                }
                else
                {
                    var stockTotal = lotesDisponibles.Sum(l => l.CantidadDisponible);
                    if (stockTotal < cantidadRequeridaEnStock)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente de '{producto.Nombre}'. " +
                            $"Requerido: {cantidadRequeridaEnStock} unidades, Disponible: {stockTotal} unidades");
                    }
                }

                var consumos = new List<ConsumoProductoDTO>();
                decimal cantidadPendiente = cantidadRequeridaEnStock;

                foreach (var lote in lotesDisponibles)
                {
                    if (cantidadPendiente <= 0) break;

                    decimal cantidadAConsumir;
                    // Relación peso/unidad propia del lote (puede diferir de la nominal del producto).
                    var ratioLote = ProductoStockRules.RatioLote(lote.CantidadDisponible, lote.PesoDisponible);

                    if (operaPorPeso)
                    {
                        cantidadAConsumir = Math.Min(lote.PesoDisponible, cantidadPendiente);

                        lote.PesoDisponible -= cantidadAConsumir;
                        if (ratioLote > 0)
                        {
                            lote.CantidadDisponible -= cantidadAConsumir / ratioLote;
                        }
                    }
                    else
                    {
                        cantidadAConsumir = Math.Min(lote.CantidadDisponible, cantidadPendiente);

                        lote.CantidadDisponible -= cantidadAConsumir;
                        if (ratioLote > 0)
                        {
                            lote.PesoDisponible -= cantidadAConsumir * ratioLote;
                        }
                    }

                    await _lotesProductosRepository.UpdateAsync(lote);

                    var costBasis = operaPorPeso
                        ? lote.PrecioKilo
                        : lote.PrecioUnitario;

                    if (operaPorPeso && lote.PrecioKilo <= 0)
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
                        // La unidad se deriva del modo, no la elige el cliente.
                        IdUnidadMedida = ProductoStockRules.UnidadMedidaId(operaPorPeso),
                        IdEntidad = idCliente == Guid.Empty ? null : idCliente,
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
                        // En el modo mixto una venta por unidad se descuenta en kg pero se informa
                        // en unidades, para que la línea de factura siga siendo coherente.
                        CantidadConsumida = factorAKg == 1m ? cantidadAConsumir : cantidadAConsumir / factorAKg,
                        UnidadMedida = unidadMedida?.Abreviatura,
                        IdMovimiento = movimiento.Id,
                        IdLote = lote.Id
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
    }
}
