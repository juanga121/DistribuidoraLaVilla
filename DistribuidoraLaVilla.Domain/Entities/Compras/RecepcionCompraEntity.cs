using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DistribuidoraLaVilla.Domain.Enums;

namespace DistribuidoraLaVilla.Domain.Entities.Compras
{
    [Table("recepciones_compra")]
    public class RecepcionCompraEntity
    {
        [Key]
        [Column("id_recepcion_compra")]
        public int IdRecepcionCompra { get; set; }

        [Column("id_orden_compra")]
        public int IdOrdenCompra { get; set; }

        [Column("id_proveedor")]
        public Guid IdProveedor { get; set; }

        [Column("numero_factura_proveedor")]
        [StringLength(50)]
        public string NumeroFacturaProveedor { get; set; } = string.Empty;

        [Column("fecha_factura")]
        public DateTime FechaFactura { get; set; }

        [Column("fecha_recepcion")]
        public DateTime FechaRecepcion { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [Column("fecha_vencimiento_lotes")]
        public DateTime FechaVencimientoLotes { get; set; }

        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Column("forma_pago")]
        public int FormaPago { get; set; } = (int)DistribuidoraLaVilla.Domain.Enums.FormaPago.Credito;

        [Column("monto_total", TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        [Column("observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

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
