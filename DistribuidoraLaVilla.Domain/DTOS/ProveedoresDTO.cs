using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class ProveedoresDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres")]
        public string? Email { get; set; }

        [Range(1, 5, ErrorMessage = "El tipo de proveedor debe estar entre 1 y 5")]
        public int? TipoProveedor { get; set; }
    }
}
