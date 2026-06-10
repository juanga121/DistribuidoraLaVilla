using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Caja
{
    [Table("recibos_caja")]
    public class ReciboEntity
    {
        [Key]
        [Column("id_recibo")]
        public int Id { get; set; }

        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("numero_recibo")]
        [StringLength(20)]
        public string NumeroRecibo { get; set; } = string.Empty;

        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; }

        [Column("id_cliente")]
        public Guid? IdCliente { get; set; }

        [Column("cliente_nombre")]
        [StringLength(100)]
        public string? ClienteNombre { get; set; }

        [Column("numero_factura")]
        [StringLength(50)]
        public string? NumeroFactura { get; set; }

        [Column("monto_pagado")]
        public decimal? MontoPagado { get; set; }

        [Column("metodo_pago")]
        public int? MetodoPago { get; set; }

        [Column("id_usuario")]
        public Guid? IdUsuario { get; set; }
    }
}
