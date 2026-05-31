using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DistribuidoraLaVilla.Domain.DTOS;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Interfaces;

namespace DistribuidoraLaVilla.Application.Services
{
    public class UsuariosService
    {
        private readonly IGenericRepository<UsuariosEntity, Guid> _repository;
        private readonly IConfiguration _configuration;

        public UsuariosService(IGenericRepository<UsuariosEntity, Guid> repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO dto)
        {
            var usuarios = _repository.GetByFilter(u => u.Email == dto.Email);
            var usuario = usuarios.FirstOrDefault();

            if (usuario is null)
                throw new Exception("Credenciales inválidas");

            var hasher = new PasswordHasher<UsuariosEntity>();
            var verificationResult = hasher.VerifyHashedPassword(null, usuario.PasswordHash, dto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                throw new Exception("Credenciales inválidas");

            if (usuario.Estado != 1)
                throw new Exception("El usuario se encuentra inactivo");

            var token = GenerateJwtToken(usuario);

            return new LoginResponseDTO
            {
                Token = token,
                Usuario = MapToResponse(usuario)
            };
        }

        public async Task<List<UsuarioResponseDTO>> ObtenerUsuariosAsync()
        {
            var usuarios = await _repository.GetAllAsync();
            return usuarios.Select(MapToResponse).ToList();
        }

        public async Task<UsuarioResponseDTO> ObtenerUsuarioPorIdAsync(Guid id)
        {
            var usuario = await _repository.FindByIdAsync(id);
            if (usuario is null)
                throw new Exception("El usuario no existe");

            return MapToResponse(usuario);
        }

        public async Task<UsuarioResponseDTO> CrearUsuarioAsync(CrearUsuarioDTO dto)
        {
            var emailExiste = _repository.GetByFilter(u => u.Email == dto.Email).Any();

            if (emailExiste)
                throw new Exception("El email ya se encuentra registrado");

            var hasher = new PasswordHasher<UsuariosEntity>();
            var passwordHash = hasher.HashPassword(null, dto.Password);

            var usuario = new UsuariosEntity
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Documento = dto.Documento,
                Telefono = dto.Telefono,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Rol = dto.Rol,
                Estado = 1
            };

            await _repository.CreateAsync(usuario);

            return MapToResponse(usuario);
        }

        public async Task<UsuarioResponseDTO> ActualizarUsuarioAsync(Guid id, ActualizarUsuarioDTO dto)
        {
            var usuario = await _repository.FindByIdAsync(id);
            if (usuario is null)
                throw new Exception("El usuario no existe");

            var emailExiste = _repository.GetByFilter(u => u.Email == dto.Email && u.Id != id).Any();

            if (emailExiste)
                throw new Exception("El email ya se encuentra registrado por otro usuario");

            usuario.Nombre = dto.Nombre;
            usuario.Documento = dto.Documento;
            usuario.Telefono = dto.Telefono;
            usuario.Email = dto.Email;
            usuario.Rol = dto.Rol;

            await _repository.UpdateAsync(usuario);

            return MapToResponse(usuario);
        }

        public async Task ActualizarEstadoUsuarioAsync(Guid id, int nuevoEstado)
        {
            var usuario = await _repository.FindByIdAsync(id);
            if (usuario is null)
                throw new Exception("El usuario no existe");

            usuario.Estado = nuevoEstado;
            await _repository.UpdateAsync(usuario);
        }

        public async Task EliminarUsuarioAsync(Guid id)
        {
            var usuario = await _repository.FindByIdAsync(id);
            if (usuario is null)
                throw new Exception("El usuario no existe");

            usuario.Estado = 0;
            await _repository.UpdateAsync(usuario);
        }

        private string GenerateJwtToken(UsuariosEntity user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new Exception("Jwt:Key no configurado");
            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new Exception("Jwt:Issuer no configurado");
            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new Exception("Jwt:Audience no configurado");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.Rol.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UsuarioResponseDTO MapToResponse(UsuariosEntity entity)
        {
            return new UsuarioResponseDTO
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Documento = entity.Documento,
                Telefono = entity.Telefono,
                Email = entity.Email,
                Rol = entity.Rol,
                Estado = entity.Estado
            };
        }
    }
}
