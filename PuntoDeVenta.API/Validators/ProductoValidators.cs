using FluentValidation;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Validators
{
    /// <summary>
    /// Validador para creacion de productos
    /// OWASP A03: Injection - Validacion robusta de entrada
    /// </summary>
    public class ProductoCreateValidator : AbstractValidator<ProductoCreateDTO>
    {
        public ProductoCreateValidator()
        {
            // Nombre
            RuleFor(x => x.Nombre)
                .NotEmpty()
                    .WithMessage("El nombre es requerido")
                .Length(2, 200)
                    .WithMessage("El nombre debe tener entre 2 y 200 caracteres")
                .Must(NotContainDangerousCharacters)
                    .WithMessage("El nombre contiene caracteres no permitidos");

            // Grupo
            RuleFor(x => x.Grupo)
                .GreaterThan(0)
                    .WithMessage("El grupo es requerido y debe ser válido");

            // Codigo
            RuleFor(x => x.Codigo)
                .NotEmpty()
                    .WithMessage("El código es requerido")
                .Length(1, 50)
                    .WithMessage("El código debe tener entre 1 y 50 caracteres")
                .Matches(@"^[a-zA-Z0-9\-_]+$")
                    .WithMessage("El código solo puede contener letras, números, guiones y guiones bajos");

            // Precio
            RuleFor(x => x.Precio)
                .GreaterThan(0)
                    .WithMessage("El precio debe ser mayor a 0")
                .LessThan(999999999)
                    .WithMessage("El precio excede el límite permitido");

            // Cantidad
            RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("La cantidad no puede ser negativa");

            // Unidad de medida
            RuleFor(x => x.UnidadMedida)
                .MaximumLength(20)
                    .WithMessage("La unidad de medida no puede exceder 20 caracteres")
                .Matches(@"^[a-zA-Z\s]+$")
                    .When(x => !string.IsNullOrEmpty(x.UnidadMedida))
                    .WithMessage("La unidad de medida solo puede contener letras");

            // Descripcion
            RuleFor(x => x.Descripcion)
                .MaximumLength(500)
                    .WithMessage("La descripción no puede exceder 500 caracteres")
                .Must(NotContainScriptTags)
                    .When(x => !string.IsNullOrEmpty(x.Descripcion))
                    .WithMessage("La descripción contiene contenido no permitido");
        }

        private bool NotContainDangerousCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            var dangerous = new[] { "<script", "javascript:", "onerror", "onload", "<iframe" };
            var lowerValue = value.ToLowerInvariant();
            foreach (var pattern in dangerous)
            {
                if (lowerValue.Contains(pattern))
                    return false;
            }
            return true;
        }

        private bool NotContainScriptTags(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            var lowerValue = value.ToLowerInvariant();
            return !lowerValue.Contains("<script") && !lowerValue.Contains("javascript:");
        }
    }

    /// <summary>
    /// Validador para actualizacion de productos
    /// </summary>
    public class ProductoUpdateValidator : AbstractValidator<ProductoUpdateDTO>
    {
        public ProductoUpdateValidator()
        {
            // Nombre (opcional pero con restricciones si se proporciona)
            RuleFor(x => x.Nombre)
                .Length(2, 200)
                    .When(x => !string.IsNullOrEmpty(x.Nombre))
                    .WithMessage("El nombre debe tener entre 2 y 200 caracteres");

            // Codigo (opcional pero con restricciones si se proporciona)
            RuleFor(x => x.Codigo)
                .Length(1, 50)
                    .When(x => !string.IsNullOrEmpty(x.Codigo))
                    .WithMessage("El código debe tener entre 1 y 50 caracteres")
                .Matches(@"^[a-zA-Z0-9\-_]+$")
                    .When(x => !string.IsNullOrEmpty(x.Codigo))
                    .WithMessage("El código solo puede contener letras, números, guiones y guiones bajos");

            // Grupo (opcional pero con restricciones si se proporciona)
            RuleFor(x => x.Grupo)
                .GreaterThan(0)
                    .When(x => x.Grupo.HasValue)
                    .WithMessage("El grupo debe ser válido");

            // Precio (opcional pero con restricciones si se proporciona)
            RuleFor(x => x.Precio)
                .GreaterThan(0)
                    .When(x => x.Precio.HasValue)
                    .WithMessage("El precio debe ser mayor a 0")
                .LessThan(999999999)
                    .When(x => x.Precio.HasValue)
                    .WithMessage("El precio excede el límite permitido");

            // Cantidad (opcional pero con restricciones si se proporciona)
            RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0)
                    .When(x => x.Cantidad.HasValue)
                    .WithMessage("La cantidad no puede ser negativa");

            // Unidad de medida
            RuleFor(x => x.UnidadMedida)
                .MaximumLength(20)
                    .When(x => !string.IsNullOrEmpty(x.UnidadMedida))
                    .WithMessage("La unidad de medida no puede exceder 20 caracteres");

            // Descripcion
            RuleFor(x => x.Descripcion)
                .MaximumLength(500)
                    .When(x => !string.IsNullOrEmpty(x.Descripcion))
                    .WithMessage("La descripción no puede exceder 500 caracteres");
        }
    }

    /// <summary>
    /// Validador para actualizacion de stock
    /// </summary>
    public class ActualizarStockValidator : AbstractValidator<ActualizarStockDTO>
    {
        public ActualizarStockValidator()
        {
            RuleFor(x => x.IdArticulo)
                .GreaterThan(0)
                    .WithMessage("El ID del artículo es requerido y debe ser válido");

            RuleFor(x => x.NuevaCantidad)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("La cantidad no puede ser negativa");

            RuleFor(x => x.Observacion)
                .MaximumLength(500)
                    .When(x => !string.IsNullOrEmpty(x.Observacion))
                    .WithMessage("La observación no puede exceder 500 caracteres");
        }
    }
}
