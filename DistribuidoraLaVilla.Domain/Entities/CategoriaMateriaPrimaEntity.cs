using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("categoria_materia_prima")]
    public class CategoriaMateriaPrimaEntity
    {
        [Key]
        [Column("id_categoria_materia_prima")]
        public int Id { get; set; }

        [Required]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}
