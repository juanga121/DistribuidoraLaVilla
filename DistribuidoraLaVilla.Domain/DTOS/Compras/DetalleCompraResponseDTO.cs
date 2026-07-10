namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class DetalleCompraResponseDTO
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
