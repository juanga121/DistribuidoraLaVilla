namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class CrearOrdenCompraDTO
    {
        public Guid IdProveedor { get; set; }
        public DateTime FechaEmision { get; set; }
        public string? Observaciones { get; set; }
        public List<CrearDetalleCompraDTO> Detalles { get; set; } = new();
    }
}
