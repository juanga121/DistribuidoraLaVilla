namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearTicketeraDTO
    {
        public List<DetalleTicketeraDTO> Detalles { get; set; } = new();
        public int FormaPago { get; set; } = 1;
        public Guid? IdCliente { get; set; }
    }

    public class DetalleTicketeraDTO
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int IdUnidadMedida { get; set; }
        public bool? EsVentaPorPeso { get; set; }
    }
}
