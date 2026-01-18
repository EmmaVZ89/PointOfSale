using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Capa_Datos;
using Capa_Entidad;

namespace PuntoDeVenta.API.Auth
{
    /// <summary>
    /// Servicio para generar y validar tokens JWT.
    ///
    /// TECNOLOGIA NUEVA: JWT (JSON Web Token)
    /// - Es un estandar abierto (RFC 7519) para transmitir informacion segura
    /// - Consiste en 3 partes: Header.Payload.Signature
    /// - Header: Tipo de token y algoritmo de firma
    /// - Payload: Claims (datos del usuario, roles, expiracion)
    /// - Signature: Firma digital para verificar autenticidad
    ///
    /// Ventajas:
    /// - Stateless: El servidor no guarda sesiones
    /// - Escalable: Funciona en arquitecturas distribuidas
    /// - Seguro: Firmado digitalmente
    /// </summary>
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService()
        {
            _secretKey = ConfigurationHelper.GetAppSetting("JwtKey")
                ?? "PuntoDeVenta_SuperSecretKey_2024_MinLength32Chars!";
            _issuer = ConfigurationHelper.GetAppSetting("JwtIssuer")
                ?? "PuntoDeVenta.API";
            _audience = ConfigurationHelper.GetAppSetting("JwtAudience")
                ?? "PuntoDeVenta.Clients";

            var expMinutes = ConfigurationHelper.GetAppSetting("JwtExpirationMinutes");
            _expirationMinutes = string.IsNullOrEmpty(expMinutes) ? 60 : int.Parse(expMinutes);
        }

        /// <summary>
        /// Genera un token JWT para un usuario autenticado
        /// </summary>
        /// <param name="usuario">Usuario autenticado</param>
        /// <returns>Token JWT</returns>
        public string GenerateToken(CE_Usuarios usuario)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims: Informacion que se incluye en el token
            var claims = new List<Claim>
            {
                // Sub (Subject): ID unico del usuario
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),

                // Name: Nombre del usuario
                new Claim(JwtRegisteredClaimNames.Name, $"{usuario.Nombre} {usuario.Apellido}"),

                // Email
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo ?? ""),

                // Jti (JWT ID): ID unico del token (para refresh tokens)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // Claims personalizados
                new Claim("usuario", usuario.Usuario),
                new Claim("privilegio", usuario.Privilegio.ToString()),

                // Role claim para autorizacion basada en roles
                new Claim(ClaimTypes.Role, usuario.Privilegio == 1 ? "Admin" : "User")
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un refresh token aleatorio
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Obtiene el ClaimsPrincipal de un token expirado (para refresh)
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateLifetime = false // Permitir tokens expirados
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token invalido");
            }

            return principal;
        }

        /// <summary>
        /// Obtiene la fecha de expiracion del token
        /// </summary>
        public DateTime GetTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_expirationMinutes);
        }
    }
}
