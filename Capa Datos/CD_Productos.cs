using System;
using System.Collections.Generic;
using Npgsql;
using System.Data;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Productos
    {
        CD_Conexion conn = new CD_Conexion();
        CE_Productos productos = new CE_Productos();

        #region BUSCAR
        public DataTable Buscar(string buscar)
        {
            DataTable dt = new DataTable();
            try
            {
                // OPTIMIZACION: No traemos la columna Img en el listado para no saturar la red
                string sql = "SELECT \"IdArticulo\", \"Nombre\", \"Grupo\", \"Codigo\", \"Precio\", \"Activo\", \"Cantidad\", \"UnidadMedida\", \"Descripcion\" " +
                             "FROM \"Articulos\" WHERE \"Nombre\" ILIKE @Nombre || '%' OR \"Codigo\" ILIKE @Nombre || '%'";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@Nombre", buscar);
                da.Fill(dt);
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return dt;
        }
        #endregion

        #region CONSULTAR
        public CE_Productos Consultar(int idProducto)
        {
            try
            {
                string sql = "SELECT * FROM \"Articulos\" WHERE \"IdArticulo\" = @IdArticulo";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdArticulo", idProducto);
                
                using (NpgsqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        this.productos.Nombre = dr["Nombre"].ToString();
                        this.productos.Grupo = Convert.ToInt32(dr["Grupo"]);
                        this.productos.Codigo = dr["Codigo"].ToString();
                        this.productos.Precio = Convert.ToDecimal(dr["Precio"]);
                        this.productos.Activo = Convert.ToBoolean(dr["Activo"]);
                        this.productos.Cantidad = Convert.ToDecimal(dr["Cantidad"]);
                        this.productos.UnidadMedida = dr["UnidadMedida"].ToString();
                        this.productos.Img = dr["Img"] != DBNull.Value ? (byte[])dr["Img"] : null;
                        this.productos.Descripcion = dr["Descripcion"].ToString();
                    }
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return this.productos;
        }
        #endregion

        #region INSERTAR
        public void CD_Insertar(CE_Productos productos, int idUsuario)
        {
            try
            {
                // Usamos la función sp_a_insertar definida en el script de Supabase
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT sp_a_insertar(@Nombre, @Grupo, @Codigo, @Precio, @Activo, @Cantidad, @UnidadMedida, @Img, @Descripcion, @IdUsuario)", this.conn.AbrirConexion());
                
                cmd.Parameters.AddWithValue("@Nombre", productos.Nombre);
                cmd.Parameters.AddWithValue("@Grupo", productos.Grupo);
                cmd.Parameters.AddWithValue("@Codigo", productos.Codigo);
                cmd.Parameters.AddWithValue("@Precio", productos.Precio);
                cmd.Parameters.AddWithValue("@Activo", productos.Activo);
                cmd.Parameters.AddWithValue("@Cantidad", productos.Cantidad);
                cmd.Parameters.AddWithValue("@UnidadMedida", productos.UnidadMedida);
                cmd.Parameters.AddWithValue("@Img", (object)productos.Img ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Descripcion", productos.Descripcion);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ELIMINAR
        public void CD_Eliminar(CE_Productos productos)
        {
            try
            {
                // Baja lógica según requerimiento anterior
                string sql = "UPDATE \"Articulos\" SET \"Activo\" = FALSE WHERE \"IdArticulo\" = @IdArticulo";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdArticulo", productos.IdArticulo);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR DATOS
        public void CD_Actualizar(CE_Productos productos)
        {
            try
            {
                string sql = "UPDATE \"Articulos\" SET \"Nombre\"=@Nombre, \"Grupo\"=@Grupo, \"Codigo\"=@Codigo, \"Precio\"=@Precio, \"Cantidad\"=@Cantidad, \"Activo\"=@Activo, \"UnidadMedida\"=@UnidadMedida, \"Descripcion\"=@Descripcion WHERE \"IdArticulo\" = @IdArticulo";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                
                cmd.Parameters.AddWithValue("@IdArticulo", productos.IdArticulo);
                cmd.Parameters.AddWithValue("@Nombre", productos.Nombre);
                cmd.Parameters.AddWithValue("@Grupo", productos.Grupo);
                cmd.Parameters.AddWithValue("@Codigo", productos.Codigo);
                cmd.Parameters.AddWithValue("@Precio", productos.Precio);
                cmd.Parameters.AddWithValue("@Cantidad", productos.Cantidad);
                cmd.Parameters.AddWithValue("@Activo", productos.Activo);
                cmd.Parameters.AddWithValue("@UnidadMedida", productos.UnidadMedida);
                cmd.Parameters.AddWithValue("@Descripcion", productos.Descripcion);
                
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR IMG
        public void CD_ActualizarIMG(CE_Productos productos)
        {
            try
            {
                string sql = "UPDATE \"Articulos\" SET \"Img\" = @Img WHERE \"IdArticulo\" = @IdArticulo";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdArticulo", productos.IdArticulo);
                cmd.Parameters.AddWithValue("@Img", (object)productos.Img ?? DBNull.Value);
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
