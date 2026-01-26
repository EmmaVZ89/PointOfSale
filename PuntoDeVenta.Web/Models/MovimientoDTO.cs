using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.Web.Models
{
    public class MovimientoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public int? IdUsuario { get; set; }
        public string UsuarioResponsable { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? Observacion { get; set; }
    }

    public class MovimientoCreateDTO
    {
        [Required(ErrorMessage = "El articulo es requerido")]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        [RegularExpression("^(ENTRADA|SALIDA|AJUSTE)$", ErrorMessage = "El tipo debe ser ENTRADA, SALIDA o AJUSTE")]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [StringLength(500)]
        public string? Observacion { get; set; }
    }

    public class MovimientoFiltroDTO
    {
        public string? Buscar { get; set; }
        public int? IdArticulo { get; set; }
        public int? IdUsuario { get; set; }
        public string? TipoMovimiento { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MovimientoResumenDTO
    {
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSalidas { get; set; }
        public UltimoMovimientoDTO? UltimoMovimiento { get; set; }
    }

    public class UltimoMovimientoDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>
    /// DTO para dashboard de inventario
    /// </summary>
    public class InventarioDashboardDTO
    {
        public int MovimientosHoy { get; set; }
        public decimal EntradasHoy { get; set; }
        public decimal SalidasHoy { get; set; }
        public decimal AjustesHoy { get; set; }
        public List<TopProductoMovidoDTO> TopProductosMovidos { get; set; } = new();
    }

    /// <summary>
    /// DTO para top productos movidos
    /// </summary>
    public class TopProductoMovidoDTO
    {
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public decimal TotalMovido { get; set; }
    }

    /// <summary>
    /// DTO para reporte de stock actual
    /// </summary>
    public class StockActualDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ValorInventario { get; set; }
        public bool StockBajo { get; set; }
    }

    /// <summary>
    /// DTO para resumen de stock
    /// </summary>
    public class StockResumenDTO
    {
        public int TotalProductos { get; set; }
        public int ProductosConStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public List<StockActualDTO> Productos { get; set; } = new();
    }
}
