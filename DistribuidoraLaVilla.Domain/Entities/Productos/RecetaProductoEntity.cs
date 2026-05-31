using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Productos
{
    [Table("recetas_producto")]
    public class RecetaProductoEntity
    {
        [Key]
        [Column("id_receta")]
        public int Id { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("id_materia_prima")]
        public int IdMateriaPrima { get; set; }

        [Column("cantidad_requerida")]
        public decimal CantidadRequerida { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
