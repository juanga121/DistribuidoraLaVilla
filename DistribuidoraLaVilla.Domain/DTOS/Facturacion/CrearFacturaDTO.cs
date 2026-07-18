namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearFacturaDTO
    {
        public Guid IdCliente { get; set; }
        public Guid IdUsuario { get; set; }
        public int TipoFactura { get; set; } = 1;
        public int FormaPago { get; set; }
        public int MetodoPago { get; set; }
        public List<CrearDetalleFacturaDTO> Detalles { get; set; } = new();
    }
}
