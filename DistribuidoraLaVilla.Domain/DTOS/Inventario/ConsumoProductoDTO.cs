namespace DistribuidoraLaVilla.Domain.DTOS.Inventario
{
    public class ConsumoProductoDTO
    {
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public decimal CantidadRequerida { get; set; }
        public decimal CantidadConsumida { get; set; }
        public string? UnidadMedida { get; set; }
        public int IdMovimiento { get; set; }
        public int IdLote { get; set; }
    }
}
