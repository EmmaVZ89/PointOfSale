using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de producto
    /// </summary>
    public class ProductoDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; }
        public int Grupo { get; set; }
        public string GrupoNombre { get; set; }
        public string Codigo { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public string Descripcion { get; set; }
        public bool TieneImagen { get; set; }
        /// <summary>
        /// Cantidad de presentaciones activas del producto
        /// </summary>
        public int CantidadPresentaciones { get; set; }

        /// <summary>
        /// Costo de compra unitario (solo visible para Admin)
        /// </summary>
        public decimal? CostoUnitario { get; set; }

        /// <summary>
        /// Margen de ganancia porcentual (solo visible para Admin)
        /// </summary>
        public decimal? MargenGanancia => CostoUnitario.HasValue && CostoUnitario > 0
            ? ((Precio - CostoUnitario.Value) / CostoUnitario.Value) * 100
            : null;
    }

    /// <summary>
    /// DTO para crear un nuevo producto
    /// </summary>
    public class ProductoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El grupo es requerido")]
        public int Grupo { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public decimal Cantidad { get; set; } = 0;

        [StringLength(20)]
        public string UnidadMedida { get; set; } = "Unidad";

        [StringLength(500)]
        public string Descripcion { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un producto
    /// </summary>
    public class ProductoUpdateDTO
    {
        [StringLength(200)]
        public string Nombre { get; set; }

        public int? Grupo { get; set; }

        [StringLength(50)]
        public string Codigo { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Precio { get; set; }

        public bool? Activo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Cantidad { get; set; }

        [StringLength(20)]
        public string UnidadMedida { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        /// <summary>
        /// Costo de compra unitario (solo Admin puede modificar)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? CostoUnitario { get; set; }
    }

    /// <summary>
    /// DTO para actualizar stock
    /// </summary>
    public class ActualizarStockDTO
    {
        [Required]
        public int IdArticulo { get; set; }

        [Required]
        public decimal NuevaCantidad { get; set; }

        public string Observacion { get; set; }
    }

    /// <summary>
    /// DTO para cambiar estado (activar/desactivar)
    /// </summary>
    public class CambiarEstadoDTO
    {
        [Required]
        public bool Activo { get; set; }
    }

    /// <summary>
    /// DTO para actualizar costo de compra unitario (solo Admin)
    /// </summary>
    public class ActualizarCostoDTO
    {
        [Required(ErrorMessage = "El costo unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
        public decimal CostoUnitario { get; set; }
    }

    /// <summary>
    /// DTO para el histórico de costos (solo Admin)
    /// </summary>
    public class CostoHistoricoDTO
    {
        public int IdCostoHistorico { get; set; }
        public int IdArticulo { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? IdUsuarioRegistro { get; set; }
        public string NombreUsuario { get; set; }
    }

    /// <summary>
    /// DTO para detalle de ganancia por producto (solo Admin)
    /// </summary>
    public class GananciaProductoDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Presentacion { get; set; }
        public int UnidadesPorPresentacion { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal UnidadesTotales { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal IngresoTotal { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal MargenPorcentaje { get; set; }
    }

    /// <summary>
    /// DTO para reporte de ganancias del día (solo Admin)
    /// </summary>
    public class ReporteGananciasDTO
    {
        public DateTime Fecha { get; set; }
        public List<GananciaProductoDTO> Productos { get; set; } = new();
        public decimal TotalIngresos { get; set; }
        public decimal TotalCostos { get; set; }
        public decimal TotalGanancias { get; set; }
        public decimal MargenPromedioGlobal { get; set; }
        public int TotalProductosVendidos { get; set; }
        public int ProductosSinCosto { get; set; }
        public string MensajeProductosSinCosto { get; set; }
    }
}
