using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Inventario
    {
        CD_Conexion conn = new CD_Conexion();

        public void RegistrarMovimiento(CE_Movimiento movimiento)
        {
            try
            {
                NpgsqlConnection connection = this.conn.AbrirConexion();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar Movimiento
                        string sqlMov = "INSERT INTO \"MovimientosInventario\" (\"IdArticulo\", \"IdUsuario\", \"TipoMovimiento\", \"Cantidad\", \"FechaMovimiento\", \"Observacion\") " +
                                        "VALUES (@IdArticulo, @IdUsuario, @TipoMovimiento, @Cantidad, @FechaMovimiento, @Observacion)";

                        NpgsqlCommand cmdMov = new NpgsqlCommand(sqlMov, connection, trans);
                        cmdMov.Parameters.AddWithValue("@IdArticulo", movimiento.IdArticulo);
                        cmdMov.Parameters.AddWithValue("@IdUsuario", movimiento.IdUsuario);
                        cmdMov.Parameters.AddWithValue("@TipoMovimiento", movimiento.TipoMovimiento);
                        cmdMov.Parameters.AddWithValue("@Cantidad", movimiento.Cantidad);
                        cmdMov.Parameters.Add(new NpgsqlParameter("@FechaMovimiento", NpgsqlDbType.Timestamp) { Value = DateTime.Now });
                        cmdMov.Parameters.AddWithValue("@Observacion", movimiento.Observacion);
                        cmdMov.ExecuteNonQuery();

                        // 2. Actualizar Stock
                        string op = movimiento.TipoMovimiento == "ENTRADA" ? "+" : "-";
                        string sqlStock = $"UPDATE \"Articulos\" SET \"Cantidad\" = \"Cantidad\" {op} @Cantidad WHERE \"IdArticulo\" = @IdArticulo";
                        
                        NpgsqlCommand cmdStock = new NpgsqlCommand(sqlStock, connection, trans);
                        cmdStock.Parameters.AddWithValue("@Cantidad", movimiento.Cantidad);
                        cmdStock.Parameters.AddWithValue("@IdArticulo", movimiento.IdArticulo);
                        cmdStock.ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                conn.CerrarConexion();
            }
        }

        public List<CE_Movimiento> ListarMovimientos(string buscar, DateTime? desde, DateTime? hasta)
        {
            List<CE_Movimiento> lista = new List<CE_Movimiento>();
            try
            {
                string sql = "SELECT m.*, a.\"Codigo\", a.\"Nombre\" AS \"NombreProducto\", u.\"usuario\" AS \"UsuarioResponsable\" " +
                             "FROM \"MovimientosInventario\" m " +
                             "LEFT JOIN \"Articulos\" a ON m.\"IdArticulo\" = a.\"IdArticulo\" " +
                             "LEFT JOIN \"Usuarios\" u ON m.\"IdUsuario\" = u.\"IdUsuario\" " +
                             "WHERE (m.\"FechaMovimiento\" BETWEEN @desde AND @hasta) " +
                             "AND (a.\"Nombre\" LIKE @buscar || '%' OR a.\"Codigo\" LIKE @buscar || '%' OR u.\"usuario\" LIKE @buscar || '%') " +
                             "ORDER BY m.\"FechaMovimiento\" DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.Add(new NpgsqlParameter("@desde", NpgsqlDbType.Timestamp) { Value = desde ?? DateTime.Now.AddDays(-30) });
                cmd.Parameters.Add(new NpgsqlParameter("@hasta", NpgsqlDbType.Timestamp) { Value = hasta ?? DateTime.Now.AddDays(1) });
                cmd.Parameters.AddWithValue("@buscar", buscar);

                using (NpgsqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new CE_Movimiento
                        {
                            IdMovimiento = Convert.ToInt32(dr["IdMovimiento"]),
                            CodigoProducto = dr["Codigo"].ToString(),
                            NombreProducto = dr["NombreProducto"].ToString(),
                            TipoMovimiento = dr["TipoMovimiento"].ToString(),
                            Cantidad = Convert.ToDecimal(dr["Cantidad"]),
                            FechaMovimiento = Convert.ToDateTime(dr["FechaMovimiento"]),
                            UsuarioResponsable = dr["UsuarioResponsable"].ToString(),
                            Observacion = dr["Observacion"].ToString()
                        });
                    }
                }
            }
            finally
            {
                conn.CerrarConexion();
            }
            return lista;
        }

        public DataSet ObtenerDatosDashboard()
        {
            DataSet ds = new DataSet();
            try
            {
                // Usar zona horaria Argentina (UTC-3) para comparar fechas correctamente
                string zonaHoraria = "(CURRENT_TIMESTAMP AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE";

                // Top 5 hoy
                string sqlTop = "SELECT a.\"Nombre\", SUM(m.\"Cantidad\") as \"TotalMovido\" " +
                                "FROM \"MovimientosInventario\" m " +
                                "INNER JOIN \"Articulos\" a ON m.\"IdArticulo\" = a.\"IdArticulo\" " +
                                $"WHERE (m.\"FechaMovimiento\" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE = {zonaHoraria} " +
                                "GROUP BY a.\"Nombre\" ORDER BY \"TotalMovido\" DESC LIMIT 5";

                NpgsqlDataAdapter da1 = new NpgsqlDataAdapter(sqlTop, conn.AbrirConexion());
                DataTable dt1 = new DataTable("TopHoy");
                da1.Fill(dt1);
                ds.Tables.Add(dt1);

                // Totales hoy
                string sqlTot = "SELECT " +
                                $"(SELECT COALESCE(SUM(\"Cantidad\"),0) FROM \"MovimientosInventario\" WHERE \"TipoMovimiento\" = 'ENTRADA' AND (\"FechaMovimiento\" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE = {zonaHoraria}) as \"EntradasHoy\", " +
                                $"(SELECT COALESCE(SUM(\"Cantidad\"),0) FROM \"MovimientosInventario\" WHERE \"TipoMovimiento\" = 'SALIDA' AND (\"FechaMovimiento\" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE = {zonaHoraria}) as \"SalidasHoy\"";

                NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sqlTot, conn.AbrirConexion());
                DataTable dt2 = new DataTable("Resumen");
                da2.Fill(dt2);
                ds.Tables.Add(dt2);
            }
            finally
            {
                conn.CerrarConexion();
            }
            return ds;
        }
    }
}