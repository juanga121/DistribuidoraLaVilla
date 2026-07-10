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
        public int CantidadVendida { get; set; }
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
        public int StockMinimo { get; set; }
    }

    public class FacturaResumenDTO
    {
        public int IdFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
