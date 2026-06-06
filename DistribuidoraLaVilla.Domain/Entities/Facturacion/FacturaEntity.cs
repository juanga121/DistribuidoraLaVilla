using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("facturas")]
    public class FacturaEntity
    {
        [Key]
        [Column("id_factura")]
        public int Id { get; set; }

        [Column("id_cliente")]
        public Guid? IdCliente { get; set; }

        [Column("id_usuario")]
        public Guid? IdUsuario { get; set; }

        [Column("fecha")]
        public DateTime? Fecha { get; set; }

        [Column("tipo_factura")]
        public int? TipoFactura { get; set; }

        [Column("forma_pago")]
        public int? FormaPago { get; set; }

        [Column("metodo_pago")]
        public int? MetodoPago { get; set; }

        [Column("total")]
        public decimal? Total { get; set; }

        [Column("estado")]
        public int? Estado { get; set; }
    }
}
