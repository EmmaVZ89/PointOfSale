using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capa_Datos.Interfaces;
using PuntoDeVenta.API.Auth;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para autenticacion.
    ///
    /// TECNOLOGIA NUEVA: ASP.NET Core Controller
    /// - [ApiController]: Habilita comportamientos especificos de API
    /// - [Route]: Define la ruta base del controller
    /// - Hereda de ControllerBase (sin vistas, solo API)
    /// - Los metodos retornan ActionResult para respuestas HTTP
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly JwtService _jwtService;

        public AuthController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _jwtService = new JwtService();
        }

        /// <summary>
        /// Login con usuario y contrasena
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT si las credenciales son validas</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar credenciales
                var usuario = await _usuarioRepository.ValidarCredencialesAsync(
                    request.Usuario,
                    request.Contrasena);

                if (usuario == null)
                {
                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Credenciales inválidas"));
                }

                if (!usuario.Activo)
                {
                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Usuario desactivado"));
                }

                // Generar tokens
                var token = _jwtService.GenerateToken(usuario);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = _jwtService.GetTokenExpiration(),
                    Usuario = new UsuarioDTO
                    {
                        IdUsuario = usuario.IdUsuario,
                        Nombre = usuario.Nombre,
                        Apellido = usuario.Apellido,
                        Dni = usuario.Dni,
                        Correo = usuario.Correo,
                        Telefono = usuario.Telefono,
                        FechaNac = usuario.Fecha_Nac,
                        Privilegio = usuario.Privilegio,
                        Activo = usuario.Activo,
                        Usuario = usuario.Usuario
                    }
                };

                return Ok(ApiResponse<LoginResponse>.Ok(response, "Login exitoso"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponse>.Error(
                    "Error interno del servidor",
                    new System.Collections.Generic.List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Login con patron de desbloqueo
        /// </summary>
        [HttpPost("login/patron")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> LoginPatron([FromBody] PatronLoginRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.ValidarPatronAsync(request.Patron);

                if (usuario == null)
                {
                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Patrón inválido"));
                }

                if (!usuario.Activo)
                {
                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Usuario desactivado"));
                }

                var token = _jwtService.GenerateToken(usuario);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = _jwtService.GetTokenExpiration(),
                    Usuario = new UsuarioDTO
                    {
                        IdUsuario = usuario.IdUsuario,
                        Nombre = usuario.Nombre,
                        Apellido = usuario.Apellido,
                        Dni = usuario.Dni,
                        Correo = usuario.Correo,
                        Telefono = usuario.Telefono,
                        FechaNac = usuario.Fecha_Nac,
                        Privilegio = usuario.Privilegio,
                        Activo = usuario.Activo,
                        Usuario = usuario.Usuario
                    }
                };

                return Ok(ApiResponse<LoginResponse>.Ok(response, "Login exitoso"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponse>.Error(
                    "Error interno del servidor",
                    new System.Collections.Generic.List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar autenticacion
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<object>> GetCurrentUser()
        {
            // El usuario viene del token JWT
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            var privilegio = User.FindFirst("privilegio")?.Value;

            return Ok(ApiResponse<object>.Ok(new
            {
                UserId = userId,
                UserName = userName,
                Privilegio = privilegio,
                IsAdmin = User.IsInRole("Admin")
            }));
        }
    }
}
