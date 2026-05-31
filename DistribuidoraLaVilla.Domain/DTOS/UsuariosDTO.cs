namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public UsuarioResponseDTO Usuario { get; set; }
    }

    public class CrearUsuarioDTO
    {
        public string Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Telefono { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Rol { get; set; }
    }

    public class ActualizarUsuarioDTO
    {
        public string Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Telefono { get; set; }
        public string Email { get; set; }
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
