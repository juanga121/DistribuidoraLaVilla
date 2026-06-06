namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class InventarioReporteDTO
    {
        public List<InventarioItemDTO> Productos { get; set; } = new();
        public List<LoteProximoVencerDTO> LotesProximosVencer { get; set; } = new();
    }

    public class InventarioItemDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public bool StockBajo { get; set; }
    }

    public class LoteProximoVencerDTO
    {
        public int IdLote { get; set; }
        public string Producto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
