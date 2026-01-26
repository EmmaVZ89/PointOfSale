using System;
using System.Collections.Generic;
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
        public int? IdUsuario { get; set; }
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
        public string Buscar { get; set; }
        public int? IdArticulo { get; set; }
        public int? IdUsuario { get; set; }
        public string TipoMovimiento { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
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
        public string NombreProducto { get; set; }
        public string CodigoProducto { get; set; }
        public decimal TotalMovido { get; set; }
    }

    /// <summary>
    /// DTO para reporte de stock actual
    /// </summary>
    public class StockActualDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Grupo { get; set; }
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
