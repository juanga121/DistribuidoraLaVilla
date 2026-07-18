using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("movimientos_financieros")]
    public class MovimientosFinancierosEntity
    {
        [Key]
        [Column("id_movimiento")]
        public int IdMovimiento { get; set; }

        [Column("tipo_movimiento")]
        [StringLength(50)]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Column("sub_tipo")]
        [StringLength(50)]
        public string? SubTipo { get; set; }

        [Column("descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column("monto", TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Column("direccion")]
        [StringLength(20)]
        public string Direccion { get; set; } = string.Empty;

        [Column("origen_modulo")]
        [StringLength(50)]
        public string? OrigenModulo { get; set; }

        [Column("referencia_id")]
        public int? ReferenciaId { get; set; }

        [Column("fecha_movimiento")]
        public DateTime FechaMovimiento { get; set; }

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
