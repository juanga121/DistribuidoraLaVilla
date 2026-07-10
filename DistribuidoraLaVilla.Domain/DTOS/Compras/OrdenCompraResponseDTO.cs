namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class OrdenCompraResponseDTO
    {
        public int Id { get; set; }
        public Guid IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public int Estado { get; set; }
        public string? NombreEstado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public List<DetalleCompraResponseDTO> Detalles { get; set; } = new();
    }
}
