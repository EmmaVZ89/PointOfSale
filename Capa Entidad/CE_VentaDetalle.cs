using System;

namespace Capa_Entidad
{
    /// <summary>
    /// Entidad para la tabla Ventas_Detalle.
    /// Representa una linea/item de una venta.
    /// </summary>
    public class CE_VentaDetalle
    {
        /// <summary>
        /// ID unico del detalle (autoincremental)
        /// </summary>
        public int Id_Detalle { get; set; }

        /// <summary>
        /// ID de la venta padre
        /// </summary>
        public int? Id_Venta { get; set; }

        /// <summary>
        /// ID del articulo/producto
        /// </summary>
        public int? Id_Articulo { get; set; }

        /// <summary>
        /// Cantidad vendida
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Precio unitario al momento de la venta (puede ser null en registros antiguos)
        /// </summary>
        public decimal? Precio_Venta { get; set; }

        /// <summary>
        /// Monto total de la linea (Cantidad * Precio)
        /// </summary>
        public decimal? Monto_Total { get; set; }

        /// <summary>
        /// ID de la presentacion vendida (nullable para ventas legacy de WPF)
        /// </summary>
        public int? IdPresentacion { get; set; }

        /// <summary>
        /// Cantidad de unidades por presentacion (1=unidad, 6=pack x6, etc.)
        /// Default 1 para compatibilidad con WPF
        /// </summary>
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;

        // Propiedades de navegacion (no mapeadas a BD, cargadas manualmente)
        public string? NombreProducto { get; set; }
        public string? CodigoProducto { get; set; }
        public string? PresentacionNombre { get; set; }
    }
}
