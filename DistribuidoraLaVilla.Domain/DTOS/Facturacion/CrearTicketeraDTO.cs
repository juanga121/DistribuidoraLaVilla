namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearTicketeraDTO
    {
        public List<DetalleTicketeraDTO> Detalles { get; set; } = new();
        public int FormaPago { get; set; } = 1; // 1=Efectivo, 2=Transferencia, 3=Tarjeta
        public Guid? IdCliente { get; set; } // null = consumidor final
    }

    public class DetalleTicketeraDTO
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int IdUnidadMedida { get; set; }
    }
}
