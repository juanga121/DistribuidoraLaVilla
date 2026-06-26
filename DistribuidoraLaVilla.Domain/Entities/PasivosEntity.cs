using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("pasivos")]
    public class PasivosEntity
    {
        [Key]
        [Column("id_pasivo")]
        public int Id { get; set; }

        [Column("nombre")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        [StringLength(255)]
        public string? Descripcion { get; set; }

        [Column("monto", TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Column("estado")]
        public int Estado { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [Column("id_usuario")]
        public Guid? IdUsuario { get; set; }
    }
}
