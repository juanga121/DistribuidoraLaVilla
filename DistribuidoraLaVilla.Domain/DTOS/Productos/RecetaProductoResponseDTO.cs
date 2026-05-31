namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO de respuesta con información completa de la receta
    /// </summary>
    public class RecetaProductoResponseDTO
    {
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int IdMateriaPrima { get; set; }
        public string? NombreMateriaPrima { get; set; }
        public decimal CantidadRequerida { get; set; }
        public int IdUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public string? SimboloUnidadMedida { get; set; }
        public int Estado { get; set; }
    }
}
