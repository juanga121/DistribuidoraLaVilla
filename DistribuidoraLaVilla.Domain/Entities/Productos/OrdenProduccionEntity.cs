using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Productos
{
    [Table("ordenes_produccion")]
    public class OrdenProduccionEntity
    {
        [Key]
        [Column("id_orden")]
        public int Id { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad_producir")]
        public decimal CantidadProducir { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("fecha_orden")]
        public DateTime FechaOrden { get; set; }

        [Column("fecha_completada")]
        public DateTime? FechaCompletada { get; set; }

        [Column("id_lote_generado")]
        public int? IdLoteGenerado { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("estado")]
        public int Estado { get; set; } // 1=Pendiente, 2=Completada, 0=Cancelada
    }
}
