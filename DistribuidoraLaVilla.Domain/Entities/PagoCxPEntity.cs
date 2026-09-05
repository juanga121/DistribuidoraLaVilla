using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("pagos_cxp")]
    public class PagoCxPEntity
    {
        [Key]
        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_cuenta_pagar")]
        public int IdCuentaPagar { get; set; }

        [Column("monto", TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; }

        [Column("id_usuario")]
        public Guid? IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("metodo_pago")]
        public int? MetodoPago { get; set; }

        [Column("observacion")]
        [MaxLength(255)]
        public string? Observacion { get; set; }
    }
}