using DistribuidoraLaVilla.Domain.DTOS.Inventario;
using DistribuidoraLaVilla.Domain.DTOS.Productos;

namespace DistribuidoraLaVilla.Application.Interfaces
{
    /// <summary>
    /// Servicio compartido de inventario para operaciones FIFO.
    /// Usado por Producción, Facturación y otros módulos que necesiten
    /// consumir stock de manera consistente.
    /// </summary>
    public interface IInventarioService
    {
        /// <summary>
        /// Consume materia prima de lotes disponibles usando FIFO
        /// (ordenado por fecha de vencimiento más próxima).
        /// Descuenta CantidadDisponible de cada lote y crea movimientos de consumo.
        /// </summary>
        /// <param name="idMateriaPrima">ID de la materia prima a consumir</param>
        /// <param name="cantidadRequerida">Cantidad total a consumir</param>
        /// <param name="idUnidadMedida">Unidad de medida</param>
        /// <param name="idUsuario">Usuario que realiza la operación</param>
        /// <param name="observacion">Descripción del movimiento</param>
        /// <returns>Lista de consumos realizados por lote</returns>
        Task<List<ConsumoIngredienteDTO>> ConsumirLotesMateriaPrimaAsync(
            int idMateriaPrima,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            string observacion);

        /// <summary>
        /// Consume productos terminados de lotes disponibles usando FIFO
        /// (ordenado por fecha de vencimiento más próxima).
        /// Descuenta CantidadDisponible de cada lote y crea movimientos en la
        /// tabla general <c>movimientos</c> con tipo_movimiento = 2 (Venta).
        /// </summary>
        /// <param name="idProducto">ID del producto a consumir</param>
        /// <param name="cantidadRequerida">Cantidad total a consumir</param>
        /// <param name="idUnidadMedida">Unidad de medida</param>
        /// <param name="idUsuario">Usuario que realiza la operación</param>
        /// <param name="idCliente">Cliente al que se le descuenta el stock</param>
        /// <param name="observacion">Descripción del movimiento</param>
        /// <param name="esVentaPorPeso">
        /// Indica si <paramref name="cantidadRequerida"/> se expresa en kilogramos (true) o en
        /// unidades (false). El modo de inventario solo decide qué contador del lote manda:
        /// en los modos por peso el stock real son los kilogramos del lote.
        /// </param>
        /// <param name="pesoRealLinea">
        /// Peso real de la mercadería en kilogramos. Obligatorio cuando un producto mixto se vende
        /// por unidad: es el peso pesado en el mostrador y mueve el stock sin conversiones.
        /// </param>
        /// <returns>Lista de consumos realizados por lote</returns>
        Task<List<ConsumoProductoDTO>> ConsumirLotesProductoAsync(
            int idProducto,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            Guid idCliente,
            string observacion,
            bool esVentaPorPeso,
            decimal? pesoRealLinea = null);
    }
}
