using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("auditoria")]
    public class AuditoriaEntity
    {
        [Key]
        [Column("id_auditoria")]
        public int Id { get; set; }

        [Column("entidad")]
        [Required]
        [MaxLength(100)]
        public string Entidad { get; set; } = string.Empty;

        [Column("id_entidad")]
        [MaxLength(50)]
        public string? IdEntidad { get; set; }

        [Column("accion")]
        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        [Column("detalle")]
        public string? Detalle { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
