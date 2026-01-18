using System;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de usuario (sin datos sensibles)
    /// </summary>
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public int Dni { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNac { get; set; }
        public int Privilegio { get; set; }
        public string PrivilegioNombre => Privilegio == 1 ? "Administrador" : "Usuario";
        public bool Activo { get; set; }
        public string Usuario { get; set; }
        // Nota: Contrasena, Patron e Img NO se exponen en el DTO
    }

    /// <summary>
    /// DTO para crear un nuevo usuario
    /// </summary>
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es requerido")]
        public int Dni { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es valido")]
        public string Correo { get; set; }

        public string Telefono { get; set; }

        public DateTime FechaNac { get; set; }

        [Range(1, 2, ErrorMessage = "El privilegio debe ser 1 (Admin) o 2 (Usuario)")]
        public int Privilegio { get; set; } = 2;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contrasena es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena debe tener entre 6 y 100 caracteres")]
        public string Contrasena { get; set; }

        public string Patron { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un usuario
    /// </summary>
    public class UsuarioUpdateDTO
    {
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(100)]
        public string Apellido { get; set; }

        public int? Dni { get; set; }

        [EmailAddress]
        public string Correo { get; set; }

        public string Telefono { get; set; }

        public DateTime? FechaNac { get; set; }

        [Range(1, 2)]
        public int? Privilegio { get; set; }

        public bool? Activo { get; set; }
    }

    /// <summary>
    /// DTO para cambiar contrasena
    /// </summary>
    public class CambiarContrasenaDTO
    {
        [Required]
        public string ContrasenaActual { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string ContrasenaNueva { get; set; }
    }
}
