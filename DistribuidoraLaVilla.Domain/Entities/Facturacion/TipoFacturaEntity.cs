using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Facturacion
{
    [Table("tipos_factura")]
    public class TipoFacturaEntity
    {
        [Key]
        [Column("id_tipo_factura")]
        public int Id { get; set; }

        [Column("nombre")]
        [StringLength(35)]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        [StringLength(255)]
        public string? Descripcion { get; set; }
    }
}
