using System;
using System.Collections.Generic;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// DTO para resumen del dashboard.
    /// Replica los KPIs del legacy: VentasHoy, Transacciones, Productos, StockBajo.
    /// </summary>
    public class DashboardResumenDTO
    {
        /// <summary>
        /// Total vendido hoy (suma de Monto_Total)
        /// </summary>
        public decimal VentasHoy { get; set; }

        /// <summary>
        /// Cantidad de ventas/transacciones hoy
        /// </summary>
        public int TransaccionesHoy { get; set; }

        /// <summary>
        /// Total de productos activos
        /// </summary>
        public int ProductosActivos { get; set; }

        /// <summary>
        /// Productos con stock <= 10
        /// </summary>
        public int ProductosBajoStock { get; set; }

        /// <summary>
        /// Ventas de los ultimos 7 dias para el grafico
        /// </summary>
        public List<VentaDiariaDTO> VentasUltimos7Dias { get; set; } = new();
    }

    /// <summary>
    /// DTO para ventas diarias (grafico semanal)
    /// </summary>
    public class VentaDiariaDTO
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CantidadVentas { get; set; }
    }

    /// <summary>
    /// DTO para productos mas vendidos
    /// </summary>
    public class ProductoTopDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }

    /// <summary>
    /// DTO para productos con stock bajo
    /// </summary>
    public class ProductoStockBajoDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
    }
}
