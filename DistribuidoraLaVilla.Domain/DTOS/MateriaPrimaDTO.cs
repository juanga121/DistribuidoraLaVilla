using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class MateriaPrimaDTO
    {
        public string? Nombre { get; set; }
        public int IdCategoria { get; set; }
    }
}
