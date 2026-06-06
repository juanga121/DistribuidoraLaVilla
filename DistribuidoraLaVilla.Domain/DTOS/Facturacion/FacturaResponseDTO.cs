namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class FacturaResponseDTO
    {
        public bool Exitoso { get; set; }
        public int IdFactura { get; set; }
        public decimal Total { get; set; }
        public string? Mensaje { get; set; }
        public FacturaTicketDTO? Ticket { get; set; }
    }
}
