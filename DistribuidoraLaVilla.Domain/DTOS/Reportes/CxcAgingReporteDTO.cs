namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class CxcAgingReporteDTO
    {
        public List<CxcBucketDTO> Buckets { get; set; } = new();
        public decimal TotalGeneral { get; set; }
    }

    public class CxcBucketDTO
    {
        public string Key { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Total { get; set; }
        public List<CxcItemDTO> Items { get; set; } = new();
    }

    public class CxcItemDTO
    {
        public int IdFactura { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
