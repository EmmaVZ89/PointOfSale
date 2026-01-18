using Capa_Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos
{
    public class CD_Carrito
    {
        CD_Conexion conn = new CD_Conexion();
        CE_Carrito carrito = new CE_Carrito();

        #region BUSCAR
        public CE_Carrito Buscar(string buscar)
        {
            try
            {
                string sql = "SELECT * FROM \"Articulos\" WHERE \"Nombre\" LIKE @buscar || '%' OR \"Codigo\" LIKE @buscar || '%' AND \"Activo\" = TRUE";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@buscar", buscar);
                DataTable dt = new DataTable();
                da.Fill(dt);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    this.carrito.Nombre = Convert.ToString(row["Nombre"]);
                    this.carrito.Precio = Convert.ToDecimal(row["Precio"]);
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return this.carrito;
        }
        #endregion

        #region AGREGAR
        public DataTable Agregar(string producto, decimal cantidad)
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT * FROM \"Articulos\" WHERE \"Codigo\" = @buscar AND \"Activo\" = TRUE";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@buscar", producto);
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    var precio = Convert.ToDecimal(dt.Rows[0]["Precio"]);
                    decimal prodTotal = cantidad * precio;

                    dt.Columns.Add("ProductoTotal", typeof(decimal));
                    foreach (DataRow row in dt.Rows)
                    {
                        row["Cantidad"] = cantidad;
                        row["ProductoTotal"] = prodTotal;
                    }
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return dt;
        }
        #endregion

        #region VENTA
        public void Venta(string factura, decimal total, DateTime fecha, int idUsuario, int idCliente = 1)
        {
            try
            {
                string sql = "INSERT INTO \"Ventas\" (\"No_Factura\", \"Fecha_Venta\", \"Monto_Total\", \"Id_Usuario\", \"Id_Cliente\") VALUES (@Factura, @Fecha, @Total, @IdUsuario, @IdCliente)";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Factura", factura);
                cmd.Parameters.Add(new NpgsqlParameter("@Fecha", NpgsqlDbType.Timestamp) { Value = fecha });
                cmd.Parameters.AddWithValue("@Total", total);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region VENTA DETALLE
        public void Venta_Detalle(string codigo, decimal cantidad, string factura, decimal totalArticulo)
        {
            try
            {
                // Llamamos a la función sp_c_venta_detalle (Debes crearla en Supabase con el script que te daré)
                string sql = "SELECT sp_c_venta_detalle(@Codigo, @Cantidad, @Factura, @Total)";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Codigo", codigo);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@Factura", factura);
                cmd.Parameters.AddWithValue("@Total", totalArticulo);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion
    }
}