namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class VentasReporteDTO
    {
        public List<VentaItemDTO> Resultados { get; set; } = new();
        public decimal TotalSum { get; set; }
        public int TotalCount { get; set; }
    }

    public class VentaItemDTO
    {
        public int IdFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }
}
