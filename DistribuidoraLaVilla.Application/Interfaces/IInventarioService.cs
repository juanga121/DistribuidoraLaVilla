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
        /// <param name="observacion">Descripción del movimiento</param>
        /// <param name="esVentaPorPeso">Indica si la venta se realiza por peso para ese detalle</param>
        /// <returns>Lista de consumos realizados por lote</returns>
        Task<List<ConsumoProductoDTO>> ConsumirLotesProductoAsync(
            int idProducto,
            decimal cantidadRequerida,
            int idUnidadMedida,
            Guid idUsuario,
            string observacion,
            bool esVentaPorPeso);
    }
}
