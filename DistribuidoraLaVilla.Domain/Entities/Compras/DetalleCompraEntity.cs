using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Compras
{
    [Table("detalle_compra")]
    public class DetalleCompraEntity
    {
        [Key]
        [Column("id_detalle")]
        public int Id { get; set; }

        [Column("id_orden_compra")]
        public int IdOrdenCompra { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad", TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
