namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class TicketeraResultDTO
    {
        public int IdFactura { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<LineaTicketDTO> Lineas { get; set; } = new();
        public DateTime Fecha { get; set; }
        public string FormaPago { get; set; } = string.Empty;
    }
}
