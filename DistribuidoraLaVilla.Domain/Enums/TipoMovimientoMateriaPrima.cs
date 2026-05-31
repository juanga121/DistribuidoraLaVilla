namespace DistribuidoraLaVilla.Domain.Enums
{
    /// <summary>
    /// Tipos de movimiento de materia prima
    /// </summary>
    public enum TipoMovimientoMateriaPrima
    {
        /// <summary>
        /// Entrada de materia prima - Suma al stock
        /// </summary>
        Entrada = 1,

        /// <summary>
        /// Consumo de materia prima en producción - Resta del stock
        /// </summary>
        Consumo = 2,

        /// <summary>
        /// Ajuste de inventario (positivo o negativo) - Modifica el stock
        /// </summary>
        Ajuste = 3,

        /// <summary>
        /// Devolución de materia prima - Suma al stock
        /// </summary>
        Devolucion = 4,

        /// <summary>
        /// Baja por vencimiento - Resta del stock
        /// </summary>
        Vencimiento = 5
    }
}
