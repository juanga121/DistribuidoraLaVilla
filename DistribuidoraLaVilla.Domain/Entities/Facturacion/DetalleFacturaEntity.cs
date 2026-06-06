using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("detalle_factura")]
    public class DetalleFacturaEntity
    {
        [Key]
        [Column("id_detalle")]
        public int Id { get; set; }

        [Column("id_factura")]
        public int? IdFactura { get; set; }

        [Column("id_producto")]
        public int? IdProducto { get; set; }

        [Column("id_lote")]
        public int? IdLote { get; set; }

        [Column("cantidad")]
        public decimal? Cantidad { get; set; }

        [Column("id_unidad_medida")]
        public int? IdUnidadMedida { get; set; }

        [Column("precio")]
        public decimal? Precio { get; set; }

        [Column("subtotal")]
        public decimal? Subtotal { get; set; }

        [Column("es_venta_por_peso")]
        public bool? EsVentaPorPeso { get; set; }

        [Column("peso_total")]
        public decimal? PesoTotal { get; set; }

        [Column("precio_kilo")]
        public decimal? PrecioKilo { get; set; }
    }
}
