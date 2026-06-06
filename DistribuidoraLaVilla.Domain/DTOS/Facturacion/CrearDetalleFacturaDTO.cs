namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearDetalleFacturaDTO
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal Precio { get; set; }
    }
}
