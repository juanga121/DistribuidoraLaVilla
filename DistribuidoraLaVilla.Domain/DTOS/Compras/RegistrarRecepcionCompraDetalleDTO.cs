namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    public class RegistrarRecepcionCompraDetalleDTO
    {
        public int IdProducto { get; set; }
        public DateTime FechaVencimientoLote { get; set; }
        public decimal CantidadUnidades { get; set; }
        public decimal PesoTotal { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioKilo { get; set; }
    }
}
