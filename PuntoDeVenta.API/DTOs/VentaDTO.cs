using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para mostrar informacion de venta
    /// </summary>
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

        /// <summary>
        /// Motivo de cancelacion (solo para ventas canceladas)
        /// </summary>
        public string? MotivoCancelacion { get; set; }

        /// <summary>
        /// Forma de pago: E = Efectivo, T = Transferencia
        /// </summary>
        public string FormaPago { get; set; } = "E";

        /// <summary>
        /// Monto recibido del cliente (solo para efectivo)
        /// </summary>
        public decimal? MontoRecibido { get; set; }

        /// <summary>
        /// Vuelto calculado (MontoRecibido - Total)
        /// </summary>
        public decimal Vuelto => MontoRecibido.HasValue ? MontoRecibido.Value - Total : 0;

        public List<VentaDetalleDTO> Detalles { get; set; } = new();
    }

    /// <summary>
    /// DTO para detalle de venta
    /// </summary>
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

    /// <summary>
    /// DTO para crear una venta
    /// </summary>
    public class VentaCreateDTO
    {
        public int? IdCliente { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Descuento { get; set; } = 0;

        public string? Observaciones { get; set; }

        /// <summary>
        /// Forma de pago: E = Efectivo, T = Transferencia
        /// </summary>
        [Required]
        [RegularExpression("^[ET]$", ErrorMessage = "Forma de pago debe ser E (Efectivo) o T (Transferencia)")]
        public string FormaPago { get; set; } = "E";

        /// <summary>
        /// Monto recibido del cliente (requerido para efectivo)
        /// </summary>
        public decimal? MontoRecibido { get; set; }

        [Required(ErrorMessage = "La venta debe tener al menos un producto")]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto")]
        public List<VentaDetalleCreateDTO> Detalles { get; set; } = new();
    }

    /// <summary>
    /// DTO para detalle de venta a crear
    /// </summary>
    public class VentaDetalleCreateDTO
    {
        [Required]
        public int IdArticulo { get; set; }

        /// <summary>
        /// ID de la presentacion (opcional, null si es venta unitaria legacy)
        /// </summary>
        public int? IdPresentacion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Unidades por presentacion (1 para unidad, 6 para pack x6, etc.)
        /// Se usa para calcular la cantidad real a descontar del stock
        /// </summary>
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;
    }

    /// <summary>
    /// DTO para filtrar ventas
    /// </summary>
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

    /// <summary>
    /// DTO para productos mas vendidos
    /// </summary>
    public class TopProductoVentaDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}
