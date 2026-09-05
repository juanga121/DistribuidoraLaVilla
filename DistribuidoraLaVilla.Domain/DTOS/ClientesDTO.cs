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
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres")]
        public string? Documento { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres")]
        public string? Direccion { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres")]
        public string? Email { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El límite de crédito no puede ser negativo")]
        public decimal? LimiteCredito { get; set; }

        [Range(0, 365, ErrorMessage = "Los días de crédito deben estar entre 0 y 365")]
        public int? DiasCredito { get; set; }

        [Range(1, 2, ErrorMessage = "El tipo de persona debe ser 1 (Natural) o 2 (Jurídica)")]
        public int? TipoPersona { get; set; }
    }
}
