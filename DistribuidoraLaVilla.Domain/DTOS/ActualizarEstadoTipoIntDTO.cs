using System;
using System.ComponentModel.DataAnnotations;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class ActualizarEstadoTipoIntDTO
    {
        [Required(ErrorMessage = "El Id es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado nuevo es requerido")]
        [Range(0, 2, ErrorMessage = "El estado debe ser 0, 1 o 2")]
        public int EstadoNuevo { get; set; }

        public Guid? IdUsuario { get; set; }
    }
}
