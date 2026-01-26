using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.Web.Models
{
    public class ProductoDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Grupo { get; set; }
        public string? GrupoNombre { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = "Unidad";
        public string? Descripcion { get; set; }
        public bool TieneImagen { get; set; }
        /// <summary>
        /// Cantidad de presentaciones activas del producto
        /// </summary>
        public int CantidadPresentaciones { get; set; }

        // Propiedades calculadas para compatibilidad con reportes de inventario
        public decimal StockMinimo => 5; // Valor por defecto si no existe en BD
        public decimal PrecioCompra => Precio * 0.7m; // Estimado como 70% del precio de venta
        public decimal PrecioVenta => Precio;
    }

    public class ProductoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grupo es requerido")]
        public int Grupo { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public decimal Cantidad { get; set; } = 0;

        [StringLength(20)]
        public string UnidadMedida { get; set; } = "Unidad";

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    public class ProductoUpdateDTO
    {
        [StringLength(200)]
        public string? Nombre { get; set; }

        public int? Grupo { get; set; }

        [StringLength(50)]
        public string? Codigo { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Precio { get; set; }

        public bool? Activo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Cantidad { get; set; }

        [StringLength(20)]
        public string? UnidadMedida { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
