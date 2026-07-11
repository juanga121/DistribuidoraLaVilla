using System;
using System.ComponentModel.DataAnnotations;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class MarcasDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }
    }
}
