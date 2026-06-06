namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearFacturaCreditoDTO
    {
        public Guid IdCliente { get; set; }
        public Guid IdUsuario { get; set; }
        public int FormaPago { get; set; }
        public int MetodoPago { get; set; }
        public int? DiasCredito { get; set; }
        public List<CrearDetalleFacturaDTO> Detalles { get; set; } = new();
    }
}
