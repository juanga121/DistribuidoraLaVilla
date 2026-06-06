using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class ClientesDTO
    {
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }

        public decimal? LimiteCredito { get; set; }

        public int? DiasCredito { get; set; }
    }
}
