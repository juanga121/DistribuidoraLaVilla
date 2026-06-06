using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities.Productos
{
    [Table("productos")]
    public class ProductosEntity
    {
        [Key]
        [Column("id_producto")]
        public int Id { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(50)]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        [StringLength(255)]
        public string? Descripcion { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("venta_por_peso")]
        public bool VentaPorPeso { get; set; }

        [Column("precio_por_kilo", TypeName = "decimal(18,2)")]
        public decimal? PrecioPorKilo { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
