using System;
using System.Data;
using Capa_Datos;

namespace Capa_Negocio
{
    public class CN_Reportes
    {
        private CD_Reportes objDatos = new CD_Reportes();

        public DataTable ObtenerVentas(DateTime desde, DateTime hasta, int? idUsuario = null, int? idCliente = null)
        {
            // Ajustar fechas
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);
            return objDatos.ReporteVentas(desde, hasta, idUsuario, idCliente);
        }

        public DataTable ObtenerStockValorizado()
        {
            return objDatos.ReporteStock();
        }

        public DataTable ObtenerTopProductos(DateTime desde, DateTime hasta)
        {
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);
            return objDatos.ReporteTopProductos(desde, hasta);
        }

        /// <summary>
        /// Obtiene resumen de ventas (una fila por venta) para reimpresión
        /// </summary>
        public DataTable ObtenerVentasResumen(DateTime desde, DateTime hasta, int? idCliente = null)
        {
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);
            return objDatos.ReporteVentasResumen(desde, hasta, idCliente);
        }

        /// <summary>
        /// Obtiene el detalle de items de una venta para reimpresión de ticket
        /// </summary>
        public DataTable ObtenerDetalleVentaParaTicket(string noFactura)
        {
            return objDatos.ObtenerDetalleVentaParaTicket(noFactura);
        }

        /// <summary>
        /// Obtiene la cabecera de una venta para reimpresión
        /// </summary>
        public DataTable ObtenerCabeceraVenta(string noFactura)
        {
            return objDatos.ObtenerCabeceraVenta(noFactura);
        }
    }
}
