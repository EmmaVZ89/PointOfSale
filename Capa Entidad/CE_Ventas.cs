using System;
using System.Collections.Generic;

namespace Capa_Entidad
{
    /// <summary>
    /// Entidad para la tabla Ventas.
    /// Representa una venta/factura completa.
    /// </summary>
    public class CE_Ventas
    {
        /// <summary>
        /// ID unico de la venta (autoincremental)
        /// </summary>
        public int Id_Venta { get; set; }

        /// <summary>
        /// Numero de factura (formato: F-YYMMDDHHmm)
        /// </summary>
        public string No_Factura { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la venta (zona horaria Argentina)
        /// </summary>
        public DateTime? Fecha_Venta { get; set; }

        /// <summary>
        /// Monto total de la venta
        /// </summary>
        public decimal Monto_Total { get; set; }

        /// <summary>
        /// ID del usuario que registro la venta
        /// </summary>
        public int? Id_Usuario { get; set; }

        /// <summary>
        /// ID del cliente (default: 1 = Consumidor Final)
        /// </summary>
        public int? Id_Cliente { get; set; }

        /// <summary>
        /// Indica si la venta fue cancelada
        /// </summary>
        public bool Cancelada { get; set; }

        /// <summary>
        /// Fecha de cancelacion (si aplica)
        /// </summary>
        public DateTime? FechaCancelacion { get; set; }

        /// <summary>
        /// ID del usuario que cancelo la venta
        /// </summary>
        public int? IdUsuarioCancelo { get; set; }

        /// <summary>
        /// Motivo de cancelacion
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

        // Propiedades de navegacion (no mapeadas a BD, cargadas manualmente)
        public string? NombreCliente { get; set; }
        public string? NombreUsuario { get; set; }
        public List<CE_VentaDetalle> Detalles { get; set; } = new();
    }
}
