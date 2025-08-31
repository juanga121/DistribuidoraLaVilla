using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("materia_prima")]
    public class MateriaPrimaEntity
    {
        [Key]
        [Column("id_materia")]
        public int Id { get; set; }

        [Required]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("id_categoria_materia_prima")]
        public int IdCategoria { get; set; }
    }
}
