using Microsoft.AspNetCore.Mvc;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Domain.DTOS;
using System.Security.Claims;

namespace DistribuidoraLaVilla.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : Controller
    {
        private readonly UsuariosService _usuariosService;

        public UsuariosController(UsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { success = false, message = "El email y la contraseña son requeridos" });

                var resultado = await _usuariosService.LoginAsync(dto);
                return Ok(new { success = true, message = "Inicio de sesión exitoso", data = resultado });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerUsuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            try
            {
                var usuarios = await _usuariosService.ObtenerUsuariosAsync();
                return Ok(new { success = true, message = "Usuarios obtenidos correctamente", data = usuarios });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ObtenerUsuarioPorId/{id}")]
        public async Task<IActionResult> ObtenerUsuarioPorId(Guid id)
        {
            try
            {
                var usuario = await _usuariosService.ObtenerUsuarioPorIdAsync(id);
                return Ok(new { success = true, message = "Usuario obtenido correctamente", data = usuario });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDTO dto)
        {
            try
            {
                var usuario = await _usuariosService.CrearUsuarioAsync(dto, GetUserId());
                return Ok(new { success = true, message = "Usuario creado exitosamente", data = usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarUsuario/{id}")]
        public async Task<IActionResult> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioDTO dto)
        {
            try
            {
                var usuario = await _usuariosService.ActualizarUsuarioAsync(id, dto, GetUserId());
                return Ok(new { success = true, message = "Usuario actualizado exitosamente", data = usuario });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe"))
                    return NotFound(new { success = false, message = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ActualizarEstadoUsuario")]
        public async Task<IActionResult> ActualizarEstadoUsuario([FromBody] ActualizarEstadoTipoGuidDTO dto)
        {
            try
            {
                await _usuariosService.ActualizarEstadoUsuarioAsync(dto.Id, dto.EstadoNuevo, GetUserId());
                return Ok(new { success = true, message = "Estado del usuario actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe"))
                    return NotFound(new { success = false, message = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarUsuario/{id}")]
        public async Task<IActionResult> EliminarUsuario(Guid id)
        {
            try
            {
                await _usuariosService.EliminarUsuarioAsync(id, GetUserId());
                return Ok(new { success = true, message = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no existe"))
                    return NotFound(new { success = false, message = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(nameIdentifier) && Guid.TryParse(nameIdentifier, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
