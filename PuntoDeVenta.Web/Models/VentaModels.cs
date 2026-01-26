using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.Web.Models
{
    public class VentaDTO
    {
        public int IdVenta { get; set; }
        public string NoFactura { get; set; } = string.Empty;
        public int? IdCliente { get; set; }
        public string? ClienteNombre { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "COMPLETADA";
        public string? Observaciones { get; set; }
        public string? MotivoCancelacion { get; set; }
        public string FormaPago { get; set; } = "E";
        public decimal? MontoRecibido { get; set; }
        public decimal Vuelto => MontoRecibido.HasValue ? MontoRecibido.Value - Total : 0;
        public List<VentaDetalleDTO> Detalles { get; set; } = new();
    }

    public class VentaDetalleDTO
    {
        public int IdDetalle { get; set; }
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Info de presentacion (para mostrar en historial)
        public int? IdPresentacion { get; set; }
        public string? PresentacionNombre { get; set; }
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;
        public decimal UnidadesTotales => Cantidad * CantidadUnidadesPorPresentacion;
    }

    public class VentaCreateDTO
    {
        public int? IdCliente { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Descuento { get; set; } = 0;

        public string? Observaciones { get; set; }

        [Required]
        public string FormaPago { get; set; } = "E";

        public decimal? MontoRecibido { get; set; }

        [Required(ErrorMessage = "La venta debe tener al menos un producto")]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto")]
        public List<VentaDetalleCreateDTO> Detalles { get; set; } = new();
    }

    public class VentaDetalleCreateDTO
    {
        [Required]
        public int IdArticulo { get; set; }

        public int? IdPresentacion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        // Unidades por presentacion (1 para unidad, 6 para pack x6, etc.)
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;
    }

    public class VentaFiltroDTO
    {
        public int? IdCliente { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Estado { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TopProductoVentaDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}
