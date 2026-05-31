namespace DistribuidoraLaVilla.Domain.Enums
{
    /// <summary>
    /// Tipos de movimiento permitidos para productos terminados
    /// </summary>
    public enum TipoMovimientoProducto
    {
        /// <summary>
        /// Entrada de inventario (suma stock)
        /// </summary>
        Entrada = 1,

        /// <summary>
        /// Venta a cliente (descuenta stock)
        /// </summary>
        Venta = 2,

        /// <summary>
        /// Ajuste de inventario (puede sumar o restar)
        /// </summary>
        Ajuste = 3,

        /// <summary>
        /// Devolución de cliente (suma stock)
        /// </summary>
        Devolucion = 4,

        /// <summary>
        /// Producto vencido (descuenta stock)
        /// </summary>
        Vencimiento = 5
    }
}
