using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("marcas")]
    public class MarcaEntity
    {
        [Key]
        [Column("id_marca")]
        public int IdMarca { get; set; }
        [Column("nombre")]
        public string? Nombre { get; set; }
        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}
