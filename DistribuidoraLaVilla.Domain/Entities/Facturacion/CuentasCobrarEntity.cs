using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("cuentas_por_cobrar")]
    public class CuentasCobrarEntity
    {
        [Key]
        [Column("id_cxc")]
        public int Id { get; set; }

        [Column("id_factura")]
        public int? IdFactura { get; set; }

        [Column("id_cliente")]
        public Guid? IdCliente { get; set; }

        [Column("fecha_emision")]
        public DateTime? FechaEmision { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("monto_total")]
        public decimal? MontoTotal { get; set; }

        [Column("saldo_pendiente")]
        public decimal? SaldoPendiente { get; set; }

        [Column("estado")]
        public int? Estado { get; set; }
    }
}
