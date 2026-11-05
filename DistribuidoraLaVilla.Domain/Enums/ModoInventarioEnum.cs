namespace DistribuidoraLaVilla.Domain.Enums
{
    /// <summary>
    /// Define cómo se administra el stock de un producto.
    /// </summary>
    public enum ModoInventarioEnum
    {
        /// <summary>Stock en unidades. Recepción y venta por unidad.</summary>
        Unidad = 1,
        /// <summary>Stock en kilogramos. Recepción y venta por peso.</summary>
        Peso = 2,

        // El valor 3 quedó reservado: era "Peso variable", un modo que no cambiaba nada respecto
        // de Peso. No se debe reutilizar para otro modo.

        /// <summary>
        /// Stock en kilogramos, vendible por kilo o por unidad. Requiere un peso por unidad fijo
        /// para poder convertir la venta por unidad a kilogramos.
        /// </summary>
        UnidadYPeso = 4
    }
}
