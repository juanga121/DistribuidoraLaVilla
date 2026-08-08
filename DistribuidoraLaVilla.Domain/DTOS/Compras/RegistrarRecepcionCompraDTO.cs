namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class RegistrarRecepcionCompraDTO
    {
        public string NumeroFacturaProveedor { get; set; } = string.Empty;
        public DateTime FechaFactura { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaVencimientoLotes { get; set; }
        public int IdMarca { get; set; }
        public string? Observaciones { get; set; }
        public List<RegistrarRecepcionCompraDetalleDTO> Detalles { get; set; } = new();
    }
}
