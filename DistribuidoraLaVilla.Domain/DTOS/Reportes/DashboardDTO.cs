namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class DashboardDTO
    {
        public VentasPeriodoDTO VentasHoy { get; set; } = new();
        public VentasPeriodoDTO VentasSemana { get; set; } = new();
        public VentasPeriodoDTO VentasMes { get; set; } = new();
        public List<TopProductoDTO> TopProductos { get; set; } = new();
        public CxcResumenDTO CxcPendiente { get; set; } = new();
        public CxcResumenDTO CxcVencido { get; set; } = new();
        public List<StockBajoDTO> StockBajo { get; set; } = new();
        public List<FacturaResumenDTO> UltimasFacturas { get; set; } = new();
        public List<string> Alertas { get; set; } = new();
        public VentasPeriodoDTO VentasContado { get; set; } = new();
        public VentasPeriodoDTO VentasCredito { get; set; } = new();
        public List<StockBajoCategoriaDTO> StockBajoPorCategoria { get; set; } = new();
    }

    public class StockBajoCategoriaDTO
    {
        public int IdCategoria { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public int ProductosBajos { get; set; }
    }

    public class VentasPeriodoDTO
    {
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public class TopProductoDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal TotalVendido { get; set; }

        /// <summary>
        /// Cantidad vendida en la unidad del producto: kilogramos en los modos por peso,
        /// unidades en el modo por unidad. Es decimal para no truncar ventas por peso.
        /// </summary>
        public decimal CantidadVendida { get; set; }

        /// <summary>Inventory mode of the product: 1 = Unidad, 2 = Peso, 3 = Peso variable.</summary>
        public int ModoInventario { get; set; } = 1;

        /// <summary>Unit of <see cref="CantidadVendida"/>: "kg" en modos por peso, "und." en modo unidad.</summary>
        public string SimboloUnidad { get; set; } = "und.";
    }

    public class CxcResumenDTO
    {
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public class StockBajoDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal StockActual { get; set; }

        /// <summary>Minimum stock before the product is reported as low, in the mode unit.</summary>
        public decimal StockMinimo { get; set; }

        /// <summary>Inventory mode of the product: 1 = Unidad, 2 = Peso, 3 = Peso variable.</summary>
        public int ModoInventario { get; set; } = 1;

        /// <summary>Unit of <see cref="StockActual"/> and <see cref="StockMinimo"/>: "kg" o "und.".</summary>
        public string SimboloUnidad { get; set; } = "und.";
    }

    public class FacturaResumenDTO
    {
        public int IdFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
