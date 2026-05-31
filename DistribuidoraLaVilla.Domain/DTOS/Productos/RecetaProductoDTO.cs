namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO para crear o actualizar una receta de producto
    /// </summary>
    public class RecetaProductoDTO
    {
        public int IdProducto { get; set; }
        public int IdMateriaPrima { get; set; }
        public decimal CantidadRequerida { get; set; }
        public int IdUnidadMedida { get; set; }
    }
}
