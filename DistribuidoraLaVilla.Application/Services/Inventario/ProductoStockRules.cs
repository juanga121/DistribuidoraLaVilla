using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Enums;

namespace DistribuidoraLaVilla.Application.Services.Inventario
{
    /// <summary>
    /// Single source of truth for how a product's stock is measured and valued.
    /// </summary>
    /// <remarks>
    /// Invariants:
    /// <list type="bullet">
    /// <item>Modes <see cref="ModoInventarioEnum.Peso"/> and <see cref="ModoInventarioEnum.UnidadYPeso"/>
    /// keep their stock in kilograms: <c>CantidadDisponible</c> and <c>CantidadInicial</c> of a lot are kg,
    /// and <c>CantidadUnidades</c> is only an informational package count that may be 0.</item>
    /// <item><see cref="ModoInventarioEnum.Unidad"/> keeps its stock in units and does not use kilograms
    /// (<c>PesoDisponible</c> stays 0).</item>
    /// <item>A lot is valued as <c>peso * precioKilo</c> in weight modes and
    /// <c>unidades * precioUnitario</c> in unit mode.</item>
    /// </list>
    /// Every service that reads, writes, converts or values stock must go through this class instead of
    /// re-deriving the rule from <c>VentaPorPeso</c> or <c>PesoPorUnidad</c>.
    /// </remarks>
    public static class ProductoStockRules
    {
        /// <summary>Unit of measure id used for kilograms.</summary>
        public const int IdUnidadMedidaKilo = 1;

        /// <summary>Unit of measure id used for units.</summary>
        public const int IdUnidadMedidaUnidad = 2;

        /// <summary>Default minimum stock for unit-mode products.</summary>
        public const decimal StockMinimoPorDefectoUnidad = 10m;

        /// <summary>Default minimum stock for weight-mode products, in kilograms.</summary>
        public const decimal StockMinimoPorDefectoPeso = 20m;

        /// <summary>
        /// Minimum stock a product must keep before being reported as low, expressed in the mode
        /// unit. Falls back to the mode default when the product does not define its own value.
        /// </summary>
        public static decimal ResolverStockMinimo(int modoInventario, decimal? stockMinimo)
        {
            if (stockMinimo.HasValue)
            {
                return stockMinimo.Value;
            }

            var operaPorPeso = modoInventario == (int)ModoInventarioEnum.Peso
                || modoInventario == (int)ModoInventarioEnum.UnidadYPeso;

            return operaPorPeso ? StockMinimoPorDefectoPeso : StockMinimoPorDefectoUnidad;
        }

        /// <summary>
        /// Resolves the effective inventory mode. Rows created before <c>modo_inventario</c> existed (or
        /// requests from an older client) carry mode 0 or 1, so the legacy <c>VentaPorPeso</c> flag is the
        /// fallback that keeps them in weight mode.
        /// </summary>
        public static int ResolverModo(int modoInventario, bool ventaPorPeso)
        {
            if (modoInventario == (int)ModoInventarioEnum.Peso
                || modoInventario == (int)ModoInventarioEnum.UnidadYPeso)
            {
                return modoInventario;
            }

            if (modoInventario == (int)ModoInventarioEnum.Unidad && !ventaPorPeso)
            {
                return (int)ModoInventarioEnum.Unidad;
            }

            // Legacy rows: no mode, or mode 1 still flagged as sold by weight.
            return ventaPorPeso ? (int)ModoInventarioEnum.Peso : (int)ModoInventarioEnum.Unidad;
        }

        /// <summary>True when the product keeps its stock in kilograms.</summary>
        public static bool OperaPorPeso(ProductosEntity producto) =>
            OperaPorPeso(producto.ModoInventario, producto.VentaPorPeso);

        /// <summary>True when the given mode keeps its stock in kilograms.</summary>
        public static bool OperaPorPeso(int modoInventario, bool ventaPorPeso) =>
            ResolverModo(modoInventario, ventaPorPeso) != (int)ModoInventarioEnum.Unidad;

        /// <summary>Unit of measure id that matches how the stock is measured.</summary>
        public static int UnidadMedidaId(bool operaPorPeso) =>
            operaPorPeso ? IdUnidadMedidaKilo : IdUnidadMedidaUnidad;

        /// <summary>
        /// True when the product can be sold both by unit and by kilogram. Those products keep their
        /// stock in kilograms and convert a unit sale with the product's fixed weight per unit.
        /// </summary>
        public static bool PermiteVentaMixta(int modoInventario, bool ventaPorPeso) =>
            ResolverModo(modoInventario, ventaPorPeso) == (int)ModoInventarioEnum.UnidadYPeso;

        /// <summary>True when the product can be sold both by unit and by kilogram.</summary>
        public static bool PermiteVentaMixta(ProductosEntity producto) =>
            PermiteVentaMixta(producto.ModoInventario, producto.VentaPorPeso);

        /// <summary>
        /// Values a stock line with the single rule used by reception, manual lots and production:
        /// weight modes value by kilogram, unit mode values by unit.
        /// </summary>
        public static decimal CalcularImporte(
            bool operaPorPeso,
            decimal cantidadUnidades,
            decimal pesoTotal,
            decimal precioUnitario,
            decimal precioKilo) =>
            operaPorPeso
                ? pesoTotal * precioKilo
                : cantidadUnidades * precioUnitario;

        /// <summary>
        /// The quantity that the lot's authoritative counter must hold for a given line:
        /// kilograms in weight modes, units in unit mode.
        /// </summary>
        public static decimal CantidadAutoritativa(
            bool operaPorPeso,
            decimal cantidadUnidades,
            decimal pesoTotal) =>
            operaPorPeso ? pesoTotal : cantidadUnidades;

        /// <summary>
        /// The kilograms a lot must expose. In weight modes the authoritative counter is already kg;
        /// in unit mode kilograms are only meaningful when the lot carries a ratio.
        /// </summary>
        public static decimal PesoDerivado(bool operaPorPeso, decimal cantidadAutoritativa, decimal ratioLote) =>
            operaPorPeso
                ? cantidadAutoritativa
                : (ratioLote > 0 ? cantidadAutoritativa * ratioLote : 0m);

        /// <summary>
        /// Weight-per-unit ratio of a lot. Returns 0 when the ratio cannot be derived, which callers must
        /// treat as "no conversion available" instead of silently dividing by zero.
        /// </summary>
        public static decimal RatioLote(decimal cantidadDisponible, decimal pesoDisponible) =>
            cantidadDisponible > 0 ? pesoDisponible / cantidadDisponible : 0m;
    }
}
