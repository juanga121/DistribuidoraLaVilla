using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("cuentas_pagar")]
    public class CuentasPagarEntity
    {
        [Key]
        [Column("id_cuenta_pagar")]
        public int IdCuentaPagar { get; set; }

        [Column("id_proveedor")]
        public Guid IdProveedor { get; set; }

        [Column("id_orden_compra")]
        public int? IdOrdenCompra { get; set; }

        [Column("monto_total", TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        [Column("saldo_pendiente", TypeName = "decimal(18,2)")]
        public decimal SaldoPendiente { get; set; }

        [Column("descripcion")]
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

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
