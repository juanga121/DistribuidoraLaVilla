using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("pagos_cuentas")]
    public class PagoCuentaEntity
    {
        [Key]
        [Column("id_pago")]
        public int Id { get; set; }

        [Column("id_cxc")]
        public int? IdCxc { get; set; }

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("monto_pago")]
        public decimal? MontoPago { get; set; }

        [Column("metodo_pago")]
        public int? MetodoPago { get; set; }

        [Column("referencia")]
        [StringLength(100)]
        public string? Referencia { get; set; }

        [Column("id_usuario")]
        public Guid? IdUsuario { get; set; }

        [Column("observacion")]
        [StringLength(255)]
        public string? Observacion { get; set; }
    }
}
