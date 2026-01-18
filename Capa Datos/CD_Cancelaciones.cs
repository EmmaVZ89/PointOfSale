using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Capa_Datos
{
    public class CD_Cancelaciones
    {
        private readonly CD_Conexion conn = new CD_Conexion();

        #region BUSCAR VENTAS CANCELABLES
        public DataTable BuscarVentas(string buscar)
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT * FROM sp_buscar_ventas_cancelables(@buscar)";

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@buscar", buscar ?? "");
                da.Fill(dt);
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return dt;
        }
        #endregion

        #region OBTENER DETALLE DE VENTA
        public DataTable ObtenerDetalleVenta(int idVenta)
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT * FROM sp_detalle_venta(@idVenta)";

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@idVenta", idVenta);
                da.Fill(dt);
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return dt;
        }
        #endregion

        #region CANCELAR VENTA
        public bool CancelarVenta(int idVenta, int idUsuario, string motivo)
        {
            bool resultado = false;
            try
            {
                string sql = "SELECT sp_cancelar_venta(@idVenta, @idUsuario, @motivo, @fecha)";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@motivo", motivo ?? "");
                cmd.Parameters.Add(new NpgsqlParameter("@fecha", NpgsqlDbType.Timestamp) { Value = DateTime.Now });

                var res = cmd.ExecuteScalar();
                resultado = res != null && Convert.ToBoolean(res);
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return resultado;
        }
        #endregion
    }
}
