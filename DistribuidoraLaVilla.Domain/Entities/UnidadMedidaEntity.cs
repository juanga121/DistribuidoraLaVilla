using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("unidad_medida")]
    public class UnidadMedidaEntity
    {
        [Key]
        [Column("id_unidad")]
        public int Id { get; set; }
        [Column("nombre")]
        public string? Nombre { get; set; }
        [Column("simbolo")]
        public string? Abreviatura { get; set; }
    }
}
