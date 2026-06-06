using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("clientes")]
    public class ClientesEntity
    {
        [Key]
        [Column("id_cliente")]
        public Guid IdCliente { get; set; }

        [Column("nombre")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        [Column("documento")]
        [StringLength(20)]
        public string? Documento { get; set; }

        [Column("direccion")]
        [StringLength(255)]
        public string? Direccion { get; set; }

        [Column("telefono")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("estado")]
        public int? Estado { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [Column("limite_credito")]
        public decimal? LimiteCredito { get; set; }

        [Column("dias_credito")]
        public int? DiasCredito { get; set; }
    }
}
