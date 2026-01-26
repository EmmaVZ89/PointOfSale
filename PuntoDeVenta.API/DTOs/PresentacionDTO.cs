using System;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de una presentacion
    /// </summary>
    public class PresentacionDTO
    {
        public int IdPresentacion { get; set; }
        public int IdArticulo { get; set; }
        public string Nombre { get; set; }
        public int CantidadUnidades { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Datos del producto (para listados)
        public string ProductoNombre { get; set; }
        public string ProductoCodigo { get; set; }

        // Precio unitario calculado (precio / cantidad)
        public decimal PrecioUnitario => CantidadUnidades > 0 ? Precio / CantidadUnidades : 0;
    }

    /// <summary>
    /// DTO para crear una nueva presentacion
    /// </summary>
    public class PresentacionCreateDTO
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La cantidad de unidades es requerida")]
        [Range(1, 1000, ErrorMessage = "La cantidad debe ser entre 1 y 1000")]
        public int CantidadUnidades { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una presentacion
    /// </summary>
    public class PresentacionUpdateDTO
    {
        [StringLength(50)]
        public string Nombre { get; set; }

        [Range(1, 1000)]
        public int? CantidadUnidades { get; set; }

        [Range(0.01, 999999999)]
        public decimal? Precio { get; set; }

        public bool? Activo { get; set; }
    }

    /// <summary>
    /// DTO para alta rapida de presentaciones predefinidas (Pack x6, x12, x18, x24)
    /// </summary>
    public class PresentacionRapidaDTO
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "El tipo de pack es requerido")]
        [RegularExpression("^(6|12|18|24)$", ErrorMessage = "El tipo de pack debe ser 6, 12, 18 o 24")]
        public int TipoPack { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }
}
