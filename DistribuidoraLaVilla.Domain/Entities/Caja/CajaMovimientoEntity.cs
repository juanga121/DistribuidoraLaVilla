using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Caja
{
    [Table("caja_movimientos")]
    public class CajaMovimientoEntity
    {
        [Key]
        [Column("id_movimiento")]
        public int Id { get; set; }

        [Column("id_apertura")]
        public int IdApertura { get; set; }

        [Column("tipo_movimiento")]
        public int TipoMovimiento { get; set; }

        [Column("id_factura")]
        public int? IdFactura { get; set; }

        [Column("id_pago")]
        public int? IdPago { get; set; }

        [Column("id_recibo")]
        public int? IdRecibo { get; set; }

        [Column("concepto")]
        [StringLength(255)]
        public string Concepto { get; set; } = string.Empty;

        [Column("monto")]
        public decimal Monto { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("metodo_pago")]
        public int? MetodoPago { get; set; }
    }
}
