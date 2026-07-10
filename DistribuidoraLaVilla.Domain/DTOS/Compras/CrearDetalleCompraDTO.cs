namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class CrearDetalleCompraDTO
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
