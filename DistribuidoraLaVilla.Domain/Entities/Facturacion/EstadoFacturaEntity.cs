using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("estados_factura")]
    public class EstadoFacturaEntity
    {
        [Key]
        [Column("id_estado_factura")]
        public int Id { get; set; }

        [Column("nombre")]
        [StringLength(35)]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        [StringLength(255)]
        public string? Descripcion { get; set; }
    }
}
