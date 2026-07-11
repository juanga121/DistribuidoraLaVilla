using System.ComponentModel.DataAnnotations;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public UsuarioResponseDTO Usuario { get; set; }
    }

    public class CrearUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres")]
        public string? Documento { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, 3, ErrorMessage = "El rol debe estar entre 1 y 3")]
        public int Rol { get; set; }
    }

    public class ActualizarUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres")]
        public string? Documento { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, 3, ErrorMessage = "El rol debe estar entre 1 y 3")]
        public int Rol { get; set; }
    }

    public class UsuarioResponseDTO
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Telefono { get; set; }
        public string Email { get; set; }
        public int Rol { get; set; }
        public int Estado { get; set; }
    }
}
