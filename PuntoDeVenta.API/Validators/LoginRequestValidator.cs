using FluentValidation;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Validators
{
    /// <summary>
    /// Validador para solicitudes de login
    /// OWASP A03: Injection - Validacion robusta de entrada
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // Usuario requerido
            RuleFor(x => x.Usuario)
                .NotEmpty()
                    .WithMessage("El usuario es requerido")
                .Length(3, 50)
                    .WithMessage("El usuario debe tener entre 3 y 50 caracteres")
                .Matches(@"^[a-zA-Z0-9._@-]+$")
                    .WithMessage("El usuario solo puede contener letras, números y los caracteres . _ @ -")
                // Prevenir injection patterns
                .Must(NotContainSqlInjectionPatterns)
                    .WithMessage("El usuario contiene caracteres no permitidos");

            // Contrasena requerida
            RuleFor(x => x.Contrasena)
                .NotEmpty()
                    .WithMessage("La contraseña es requerida")
                .MinimumLength(4)
                    .WithMessage("La contraseña debe tener al menos 4 caracteres")
                .MaximumLength(100)
                    .WithMessage("La contraseña no puede exceder 100 caracteres");
        }

        /// <summary>
        /// Verifica que el valor no contenga patrones de SQL injection
        /// </summary>
        private bool NotContainSqlInjectionPatterns(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            var lowerValue = value.ToLowerInvariant();

            // Patrones comunes de SQL injection
            var patterns = new[]
            {
                "'",
                "\"",
                ";",
                "--",
                "/*",
                "*/",
                "xp_",
                "sp_",
                "exec",
                "execute",
                "insert",
                "select",
                "delete",
                "update",
                "drop",
                "union",
                "declare"
            };

            foreach (var pattern in patterns)
            {
                if (lowerValue.Contains(pattern))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validador para solicitudes de login con patron
    /// </summary>
    public class PatronLoginRequestValidator : AbstractValidator<PatronLoginRequest>
    {
        public PatronLoginRequestValidator()
        {
            RuleFor(x => x.Patron)
                .NotEmpty()
                    .WithMessage("El patrón es requerido")
                .Length(4, 100)
                    .WithMessage("El patrón debe tener entre 4 y 100 caracteres");
        }
    }
}
