using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("usuarios")]
    public class UsuariosEntity
    {
        [Key]
        [Column("id_usuario")]
        public Guid Id { get; set; }

        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Column("documento")]
        [StringLength(20)]
        public string? Documento { get; set; }

        [Column("telefono")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("rol")]
        public int Rol { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
