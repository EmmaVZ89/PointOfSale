using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.Web.Models
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public int Dni { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaNac { get; set; }
        public int Privilegio { get; set; }
        public string PrivilegioNombre => Privilegio == 1 ? "Administrador" : "Usuario";
        public bool Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es requerido")]
        public int Dni { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public DateTime FechaNac { get; set; } = DateTime.Now.AddYears(-18);

        [Range(1, 2, ErrorMessage = "El privilegio debe ser 1 (Admin) o 2 (Usuario)")]
        public int Privilegio { get; set; } = 2;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Contrasena { get; set; } = string.Empty;

        public string? Patron { get; set; }
    }

    public class UsuarioUpdateDTO
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        [StringLength(100)]
        public string? Apellido { get; set; }

        public int? Dni { get; set; }

        [EmailAddress]
        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public DateTime? FechaNac { get; set; }

        [Range(1, 2)]
        public int? Privilegio { get; set; }

        public bool? Activo { get; set; }
    }

    public class CambiarContrasenaDTO
    {
        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
