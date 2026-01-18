using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Capa_Datos
{
    public class CD_Reportes
    {
        private CD_Conexion conn = new CD_Conexion();

        public DataTable ReporteVentas(DateTime desde, DateTime hasta, int? idUsuario, int? idCliente = null)
        {
            // Ajustar 'hasta' para incluir todo el día (23:59:59)
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);

            // Convertir Fecha_Venta a zona horaria Argentina para comparación correcta
            string sql = "SELECT v.\"No_Factura\" AS \"Nro Factura\", " +
                         "(v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') AS \"Fecha Venta\", " +
                         "u.\"usuario\" AS \"Vendedor\", COALESCE(c.\"RazonSocial\", 'Consumidor Final') AS \"Cliente\", " +
                         "a.\"Nombre\" AS \"Producto\", vd.\"Cantidad\" AS \"Unidades\", " +
                         "vd.\"Precio_Venta\" AS \"Precio Unit.\", vd.\"Monto_Total\" AS \"Subtotal\" " +
                         "FROM \"Ventas\" v " +
                         "INNER JOIN \"Ventas_Detalle\" vd ON v.\"Id_Venta\" = vd.\"Id_Venta\" " +
                         "INNER JOIN \"Articulos\" a ON vd.\"Id_Articulo\" = a.\"IdArticulo\" " +
                         "INNER JOIN \"Usuarios\" u ON v.\"Id_Usuario\" = u.\"IdUsuario\" " +
                         "LEFT JOIN \"Clientes\" c ON v.\"Id_Cliente\" = c.\"IdCliente\" " +
                         "WHERE ((v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') BETWEEN @desde AND @hasta) " +
                         "AND (v.\"Cancelada\" = FALSE OR v.\"Cancelada\" IS NULL) ";

            if (idUsuario.HasValue && idUsuario.Value > 0)
            {
                sql += "AND v.\"Id_Usuario\" = @IdUsuario ";
            }

            if (idCliente.HasValue && idCliente.Value > 0)
            {
                sql += "AND v.\"Id_Cliente\" = @IdCliente ";
            }

            sql += "ORDER BY v.\"Fecha_Venta\" DESC";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@desde", NpgsqlDbType.Timestamp) { Value = desde });
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@hasta", NpgsqlDbType.Timestamp) { Value = hasta });

                if (idUsuario.HasValue && idUsuario.Value > 0)
                {
                    da.SelectCommand.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);
                }

                if (idCliente.HasValue && idCliente.Value > 0)
                {
                    da.SelectCommand.Parameters.AddWithValue("@IdCliente", idCliente.Value);
                }

                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }

        public DataTable ReporteStock()
        {
            string sql = "SELECT a.\"Codigo\" AS \"Código\", a.\"Nombre\" AS \"Producto\", g.\"Nombre\" AS \"Categoría\", a.\"Cantidad\" AS \"Stock Actual\", a.\"Precio\" AS \"Precio Unit.\", (a.\"Cantidad\" * a.\"Precio\") AS \"Valor Total\" " +
                         "FROM \"Articulos\" a " +
                         "INNER JOIN \"Grupos\" g ON a.\"Grupo\" = g.\"IdGrupo\" " +
                         "WHERE a.\"Activo\" = TRUE " +
                         "ORDER BY \"Valor Total\" DESC";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }

        public DataTable ReporteTopProductos(DateTime desde, DateTime hasta)
        {
            // Ajustar 'hasta' para incluir todo el día (23:59:59)
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);

            string sql = "SELECT a.\"Nombre\" AS \"Producto\", SUM(vd.\"Cantidad\") AS \"Unidades Vendidas\", SUM(vd.\"Monto_Total\") AS \"Ingresos Totales\" " +
                         "FROM \"Ventas_Detalle\" vd " +
                         "INNER JOIN \"Ventas\" v ON vd.\"Id_Venta\" = v.\"Id_Venta\" " +
                         "INNER JOIN \"Articulos\" a ON vd.\"Id_Articulo\" = a.\"IdArticulo\" " +
                         "WHERE ((v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') BETWEEN @desde AND @hasta) " +
                         "AND (v.\"Cancelada\" = FALSE OR v.\"Cancelada\" IS NULL) " +
                         "GROUP BY a.\"Nombre\" " +
                         "ORDER BY \"Unidades Vendidas\" DESC LIMIT 10";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@desde", NpgsqlDbType.Timestamp) { Value = desde });
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@hasta", NpgsqlDbType.Timestamp) { Value = hasta });
                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }

        /// <summary>
        /// Obtiene resumen de ventas (una fila por venta) para selección de reimpresión
        /// </summary>
        public DataTable ReporteVentasResumen(DateTime desde, DateTime hasta, int? idCliente = null)
        {
            // Ajustar 'hasta' para incluir todo el día (23:59:59)
            hasta = hasta.Date.AddDays(1).AddSeconds(-1);

            string sql = "SELECT v.\"No_Factura\" AS \"Nro Factura\", " +
                         "(v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') AS \"Fecha\", " +
                         "u.\"usuario\" AS \"Vendedor\", COALESCE(c.\"RazonSocial\", 'Consumidor Final') AS \"Cliente\", " +
                         "v.\"Monto_Total\" AS \"Total\" " +
                         "FROM \"Ventas\" v " +
                         "INNER JOIN \"Usuarios\" u ON v.\"Id_Usuario\" = u.\"IdUsuario\" " +
                         "LEFT JOIN \"Clientes\" c ON v.\"Id_Cliente\" = c.\"IdCliente\" " +
                         "WHERE ((v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') BETWEEN @desde AND @hasta) " +
                         "AND (v.\"Cancelada\" = FALSE OR v.\"Cancelada\" IS NULL) ";

            if (idCliente.HasValue && idCliente.Value > 0)
            {
                sql += "AND v.\"Id_Cliente\" = @IdCliente ";
            }

            sql += "ORDER BY v.\"Fecha_Venta\" DESC";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@desde", NpgsqlDbType.Timestamp) { Value = desde });
                da.SelectCommand.Parameters.Add(new NpgsqlParameter("@hasta", NpgsqlDbType.Timestamp) { Value = hasta });

                if (idCliente.HasValue && idCliente.Value > 0)
                {
                    da.SelectCommand.Parameters.AddWithValue("@IdCliente", idCliente.Value);
                }

                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }

        /// <summary>
        /// Obtiene el detalle de una venta específica para reimpresión de ticket
        /// </summary>
        public DataTable ObtenerDetalleVentaParaTicket(string noFactura)
        {
            string sql = "SELECT a.\"Codigo\", a.\"Nombre\", vd.\"Cantidad\", " +
                         "COALESCE(vd.\"Precio_Venta\", a.\"Precio\") AS \"Precio\", " +
                         "vd.\"Monto_Total\" AS \"Total\" " +
                         "FROM \"Ventas_Detalle\" vd " +
                         "INNER JOIN \"Articulos\" a ON vd.\"Id_Articulo\" = a.\"IdArticulo\" " +
                         "INNER JOIN \"Ventas\" v ON vd.\"Id_Venta\" = v.\"Id_Venta\" " +
                         "WHERE v.\"No_Factura\" = @NoFactura";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@NoFactura", noFactura);
                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }

        /// <summary>
        /// Obtiene información de cabecera de una venta para reimpresión
        /// </summary>
        public DataTable ObtenerCabeceraVenta(string noFactura)
        {
            // Convertir fecha a zona horaria Argentina para mostrar correctamente
            string sql = "SELECT v.\"No_Factura\", " +
                         "(v.\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires') AS \"Fecha_Venta\", " +
                         "v.\"Monto_Total\", u.\"usuario\" AS \"Vendedor\" " +
                         "FROM \"Ventas\" v " +
                         "INNER JOIN \"Usuarios\" u ON v.\"Id_Usuario\" = u.\"IdUsuario\" " +
                         "WHERE v.\"No_Factura\" = @NoFactura";

            DataTable dt = new DataTable();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@NoFactura", noFactura);
                da.Fill(dt);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return dt;
        }
    }
}
