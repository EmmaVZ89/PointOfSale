using System;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de movimiento de inventario
    /// </summary>
    public class MovimientoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; }
        public string CodigoProducto { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioResponsable { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Observacion { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo movimiento
    /// </summary>
    public class MovimientoCreateDTO
    {
        [Required(ErrorMessage = "El articulo es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        [RegularExpression("^(ENTRADA|SALIDA|AJUSTE)$", ErrorMessage = "El tipo debe ser ENTRADA, SALIDA o AJUSTE")]
        public string TipoMovimiento { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [StringLength(500)]
        public string Observacion { get; set; }
    }

    /// <summary>
    /// DTO para filtrar movimientos
    /// </summary>
    public class MovimientoFiltroDTO
    {
        public int? IdArticulo { get; set; }
        public int? IdUsuario { get; set; }
        public string TipoMovimiento { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
