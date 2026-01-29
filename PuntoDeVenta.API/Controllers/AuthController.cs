using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Capa_Datos.Interfaces;
using PuntoDeVenta.API.Auth;
using PuntoDeVenta.API.DTOs;
using PuntoDeVenta.API.Security;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para autenticacion con proteccion OWASP A07
    /// Incluye: Rate limiting, brute force protection, audit logging
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly JwtService _jwtService;
        private readonly ILoginAttemptTracker _loginAttemptTracker;
        private readonly IAuditLogger _auditLogger;

        public AuthController(
            IUsuarioRepository usuarioRepository,
            ILoginAttemptTracker loginAttemptTracker,
            IAuditLogger auditLogger)
        {
            _usuarioRepository = usuarioRepository;
            _jwtService = new JwtService();
            _loginAttemptTracker = loginAttemptTracker;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// Login con usuario y contrasena
        /// Protegido con rate limiting y tracking de intentos fallidos
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingConfiguration.LoginPolicy)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var username = request?.Usuario ?? "unknown";

            try
            {
                // 1. Verificar si el usuario/IP esta bloqueado (OWASP A07)
                var attemptResult = _loginAttemptTracker.CheckAttempt(username, ipAddress);
                if (attemptResult.IsLocked)
                {
                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginLocked,
                        username,
                        ipAddress,
                        $"Cuenta bloqueada hasta {attemptResult.LockoutEndsAt:HH:mm:ss}");

                    var lockoutMinutes = (int)(attemptResult.LockoutEndsAt.Value - DateTime.UtcNow).TotalMinutes + 1;
                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        $"Cuenta bloqueada temporalmente por múltiples intentos fallidos. " +
                        $"Intente nuevamente en {lockoutMinutes} minutos."));
                }

                // 2. Validar credenciales
                var usuario = await _usuarioRepository.ValidarCredencialesAsync(
                    request.Usuario,
                    request.Contrasena);

                if (usuario == null)
                {
                    // Registrar intento fallido
                    _loginAttemptTracker.RecordFailedAttempt(username, ipAddress);
                    var newAttemptResult = _loginAttemptTracker.CheckAttempt(username, ipAddress);

                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginFailed,
                        username,
                        ipAddress,
                        $"Credenciales inválidas. Intentos restantes: {newAttemptResult.RemainingAttempts}");

                    // Mensaje con intentos restantes
                    var message = newAttemptResult.RemainingAttempts > 0
                        ? $"Credenciales inválidas. Le quedan {newAttemptResult.RemainingAttempts} intentos."
                        : "Credenciales inválidas. Su cuenta ha sido bloqueada temporalmente.";

                    return Unauthorized(ApiResponse<LoginResponse>.Error(message));
                }

                // 3. Verificar si usuario esta activo
                if (!usuario.Activo)
                {
                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginFailed,
                        username,
                        ipAddress,
                        "Usuario desactivado");

                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Usuario desactivado. Contacte al administrador."));
                }

                // 4. Login exitoso - resetear contador de intentos
                _loginAttemptTracker.ResetAttempts(username, ipAddress);

                // 5. Generar tokens
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

                // 6. Audit log de login exitoso
                _auditLogger.LogAuthentication(
                    AuditEventType.LoginSuccess,
                    username,
                    ipAddress,
                    $"Usuario ID: {usuario.IdUsuario}, Privilegio: {usuario.Privilegio}");

                return Ok(ApiResponse<LoginResponse>.Ok(response, "Login exitoso"));
            }
            catch (Exception ex)
            {
                _auditLogger.LogSecurity(
                    AuditEventType.SecurityException,
                    ipAddress,
                    $"Error en login para usuario '{username}': {ex.Message}");

                // No exponer detalles del error al cliente
                return StatusCode(500, ApiResponse<LoginResponse>.Error(
                    "Error interno del servidor. Por favor, intente nuevamente."));
            }
        }

        /// <summary>
        /// Login con patron de desbloqueo
        /// </summary>
        [HttpPost("login/patron")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingConfiguration.LoginPolicy)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> LoginPatron([FromBody] PatronLoginRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var patronHash = request?.Patron?.GetHashCode().ToString() ?? "unknown";

            try
            {
                // Verificar bloqueo por IP (para login con patron usamos IP como identificador)
                var attemptResult = _loginAttemptTracker.CheckAttempt(patronHash, ipAddress);
                if (attemptResult.IsLocked)
                {
                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginLocked,
                        "patron_login",
                        ipAddress,
                        "Bloqueado por múltiples intentos fallidos");

                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Acceso bloqueado temporalmente. Intente nuevamente más tarde."));
                }

                var usuario = await _usuarioRepository.ValidarPatronAsync(request.Patron);

                if (usuario == null)
                {
                    _loginAttemptTracker.RecordFailedAttempt(patronHash, ipAddress);
                    var newAttemptResult = _loginAttemptTracker.CheckAttempt(patronHash, ipAddress);

                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginFailed,
                        "patron_login",
                        ipAddress,
                        $"Patrón inválido. Intentos restantes: {newAttemptResult.RemainingAttempts}");

                    var message = newAttemptResult.RemainingAttempts > 0
                        ? $"Patrón inválido. Le quedan {newAttemptResult.RemainingAttempts} intentos."
                        : "Patrón inválido. Acceso bloqueado temporalmente.";

                    return Unauthorized(ApiResponse<LoginResponse>.Error(message));
                }

                if (!usuario.Activo)
                {
                    _auditLogger.LogAuthentication(
                        AuditEventType.LoginFailed,
                        usuario.Usuario,
                        ipAddress,
                        "Usuario desactivado (login patrón)");

                    return Unauthorized(ApiResponse<LoginResponse>.Error(
                        "Usuario desactivado. Contacte al administrador."));
                }

                // Login exitoso
                _loginAttemptTracker.ResetAttempts(patronHash, ipAddress);

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

                _auditLogger.LogAuthentication(
                    AuditEventType.LoginSuccess,
                    usuario.Usuario,
                    ipAddress,
                    $"Login por patrón. Usuario ID: {usuario.IdUsuario}");

                return Ok(ApiResponse<LoginResponse>.Ok(response, "Login exitoso"));
            }
            catch (Exception ex)
            {
                _auditLogger.LogSecurity(
                    AuditEventType.SecurityException,
                    ipAddress,
                    $"Error en login por patrón: {ex.Message}");

                return StatusCode(500, ApiResponse<LoginResponse>.Error(
                    "Error interno del servidor. Por favor, intente nuevamente."));
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar autenticacion
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<object>> GetCurrentUser()
        {
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

        /// <summary>
        /// Obtiene la IP del cliente considerando proxies
        /// </summary>
        private string GetClientIpAddress()
        {
            // X-Forwarded-For para Railway y otros proxies
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
