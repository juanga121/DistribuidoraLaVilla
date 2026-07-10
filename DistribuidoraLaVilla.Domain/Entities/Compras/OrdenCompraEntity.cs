using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Compras
{
    [Table("ordenes_compra")]
    public class OrdenCompraEntity
    {
        [Key]
        [Column("id_orden_compra")]
        public int Id { get; set; }

        [Column("id_proveedor")]
        public Guid IdProveedor { get; set; }

        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; }

        [Column("fecha_recepcion")]
        public DateTime? FechaRecepcion { get; set; }

        [Column("estado")]
        public int Estado { get; set; }

        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column("descuento", TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Column("impuesto", TypeName = "decimal(18,2)")]
        public decimal Impuesto { get; set; }

        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column("observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }
    }
}
