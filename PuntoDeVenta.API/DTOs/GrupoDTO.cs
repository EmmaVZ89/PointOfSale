using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de grupo/categoria
    /// </summary>
    public class GrupoDTO
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; }
        public int CantidadProductos { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo grupo
    /// </summary>
    public class GrupoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un grupo
    /// </summary>
    public class GrupoUpdateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }
    }
}
