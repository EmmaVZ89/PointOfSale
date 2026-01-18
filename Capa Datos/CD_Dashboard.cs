using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos
{
    public class CD_Dashboard
    {
        CD_Conexion conn = new CD_Conexion();

        public DataTable ObtenerResumenDia()
        {
            // Usar zona horaria Argentina (UTC-3) para comparar fechas correctamente
            string sql = "SELECT COALESCE(SUM(\"Monto_Total\"), 0) AS \"TotalVendido\", COUNT(*) AS \"CantidadVentas\" " +
                         "FROM \"Ventas\" " +
                         "WHERE (\"Fecha_Venta\" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE = " +
                         "(CURRENT_TIMESTAMP AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE " +
                         "AND (\"Cancelada\" = FALSE OR \"Cancelada\" IS NULL)";
            return EjecutarConsulta(sql);
        }

        public DataTable ObtenerResumenInventario()
        {
            string sql = "SELECT (SELECT COUNT(*) FROM \"Articulos\" WHERE \"Activo\" = TRUE) AS \"TotalProductos\", " +
                         "(SELECT COUNT(*) FROM \"Articulos\" WHERE \"Activo\" = TRUE AND \"Cantidad\" <= 10) AS \"StockBajo\"";
            return EjecutarConsulta(sql);
        }

        public DataTable ObtenerVentasSemanales()
        {
            // Llamamos a la función definida en el script de Supabase
            string sql = "SELECT * FROM sp_d_ventassemanales()";
            return EjecutarConsulta(sql);
        }

        private DataTable EjecutarConsulta(string sql)
        {
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
    }
}
