namespace PuntoDeVenta.Web.Models
{
    /// <summary>
    /// Modelo para el resumen del dashboard.
    /// Replica los KPIs del legacy.
    /// </summary>
    public class DashboardResumen
    {
        /// <summary>
        /// Total vendido hoy (formato moneda)
        /// </summary>
        public decimal VentasHoy { get; set; }

        /// <summary>
        /// Cantidad de transacciones/ventas hoy
        /// </summary>
        public int TransaccionesHoy { get; set; }

        /// <summary>
        /// Total de productos activos
        /// </summary>
        public int ProductosActivos { get; set; }

        /// <summary>
        /// Productos con stock bajo (<=10)
        /// </summary>
        public int ProductosBajoStock { get; set; }

        /// <summary>
        /// Datos para el grafico de ventas semanales
        /// </summary>
        public List<VentaDiaria> VentasUltimos7Dias { get; set; } = new();
    }

    public class VentaDiaria
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CantidadVentas { get; set; }
    }

    public class ProductoTop
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }

    public class ProductoStockBajo
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
    }
}
