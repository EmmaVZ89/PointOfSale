using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Contrasena { get; set; }
    }

    /// <summary>
    /// DTO para login con patron
    /// </summary>
    public class PatronLoginRequest
    {
        [Required(ErrorMessage = "El patron es requerido")]
        public string Patron { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de login exitoso
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public System.DateTime Expiration { get; set; }
        public UsuarioDTO Usuario { get; set; }
    }

    /// <summary>
    /// DTO para refresh token
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
