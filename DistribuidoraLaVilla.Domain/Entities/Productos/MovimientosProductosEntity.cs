using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Productos
{
    [Table("movimientos_productos")]
    public class MovimientosProductosEntity
    {
        [Key]
        [Column("id_movimiento")]
        public int Id { get; set; }

        [Column("id_lote_producto")]
        public int IdLoteProducto { get; set; }

        [Column("tipo_movimiento")]
        public int TipoMovimiento { get; set; }

        [Column("fecha_movimiento")]
        public DateTime FechaMovimiento { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("total_movimiento")]
        public decimal TotalMovimiento { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("id_cliente")]
        public Guid? IdCliente { get; set; }

        [Column("id_proveedor")]
        public Guid? IdProveedor { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
