using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Usuarios
    {
        private readonly CD_Conexion conn = new CD_Conexion();
        private readonly CE_Usuarios capaEntidadUsuarios = new CE_Usuarios();

        // CRUD Usuarios

        #region INSERTAR
        public void CD_Insertar(CE_Usuarios usuario)
        {
            try
            {
                // En Postgres usamos pgp_sym_encrypt para emular ENCRYPTBYPASSPHRASE
                string sql = "INSERT INTO \"Usuarios\" (\"Nombre\", \"Apellido\", \"DNI\", \"CUIT\", \"Correo\", \"Telefono\", \"Fecha_Nac\", \"Privilegio\", \"img\", \"usuario\", \"contrasenia\", \"Activo\") " +
                             "VALUES (@Nombre, @Apellido, @DNI, @CUIT, @Correo, @Telefono, @Fecha_Nac, @Privilegio, @Img, @usuario, pgp_sym_encrypt(@contrasenia, @patron), TRUE)";
                
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@DNI", usuario.Dni);
                cmd.Parameters.AddWithValue("@CUIT", usuario.Cuit);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono.ToString()); // Postgres espera VARCHAR(20) según script
                cmd.Parameters.Add("@Fecha_Nac", NpgsqlDbType.Date).Value = usuario.Fecha_Nac;
                cmd.Parameters.AddWithValue("@Privilegio", usuario.Privilegio);
                cmd.Parameters.AddWithValue("@Img", (object)usuario.Img ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@usuario", usuario.Usuario);
                cmd.Parameters.AddWithValue("@contrasenia", usuario.Contrasenia);
                cmd.Parameters.AddWithValue("@patron", usuario.Patron);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region CONSULTAR
        public CE_Usuarios CD_Consultar(int IdUsuario)
        {
            // Nota: Se usa comillas dobles para respetar el Case Sensitivity de Postgres definido en el script
            string sql = "SELECT \"Nombre\", \"Apellido\", \"DNI\", \"CUIT\", \"Correo\", \"Telefono\", \"Fecha_Nac\", \"Privilegio\", \"img\", \"usuario\", \"Activo\" FROM \"Usuarios\" WHERE \"IdUsuario\" = @IdUsuario";
            
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
            adapter.SelectCommand.Parameters.AddWithValue("@IdUsuario", IdUsuario);
            
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                this.capaEntidadUsuarios.Nombre = Convert.ToString(row["Nombre"]);
                this.capaEntidadUsuarios.Apellido = Convert.ToString(row["Apellido"]);
                this.capaEntidadUsuarios.Dni = Convert.ToInt32(row["DNI"]);
                this.capaEntidadUsuarios.Cuit = Convert.ToDouble(row["CUIT"]);
                this.capaEntidadUsuarios.Correo = Convert.ToString(row["Correo"]);
                this.capaEntidadUsuarios.Telefono = Convert.ToInt32(row["Telefono"]);
                this.capaEntidadUsuarios.Fecha_Nac = Convert.ToDateTime(row["Fecha_Nac"]);
                this.capaEntidadUsuarios.Privilegio = Convert.ToInt32(row["Privilegio"]);
                this.capaEntidadUsuarios.Img = row["img"] != DBNull.Value ? (byte[])row["img"] : null;
                this.capaEntidadUsuarios.Usuario = Convert.ToString(row["usuario"]);
                this.capaEntidadUsuarios.Activo = Convert.ToBoolean(row["Activo"]);
            }

            return this.capaEntidadUsuarios;
        }
        #endregion

        #region REACTIVAR
        public void CD_Reactivar(CE_Usuarios usuario)
        {
            try
            {
                string sql = "UPDATE \"Usuarios\" SET \"Activo\" = TRUE WHERE \"IdUsuario\" = @IdUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ELIMINAR
        public void CD_Eliminar(CE_Usuarios usuario)
        {
            try
            {
                string sql = "UPDATE \"Usuarios\" SET \"Activo\" = FALSE WHERE \"IdUsuario\" = @IdUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR DATOS
        public void CD_ActualizarDatos(CE_Usuarios usuario)
        {
            try
            {
                string sql = "UPDATE \"Usuarios\" SET \"Nombre\"=@Nombre, \"Apellido\"=@Apellido, \"DNI\"=@DNI, \"CUIT\"=@CUIT, \"Correo\"=@Correo, \"Telefono\"=@Telefono, \"Fecha_Nac\"=@Fecha_Nac, \"Privilegio\"=@Privilegio, \"usuario\"=@usuario WHERE \"IdUsuario\" = @IdUsuario";
                
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                cmd.Parameters.AddWithValue("@DNI", usuario.Dni);
                cmd.Parameters.AddWithValue("@CUIT", usuario.Cuit);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono.ToString());
                cmd.Parameters.Add("@Fecha_Nac", NpgsqlDbType.Date).Value = usuario.Fecha_Nac;
                cmd.Parameters.AddWithValue("@Privilegio", usuario.Privilegio);
                cmd.Parameters.AddWithValue("@usuario", usuario.Usuario);
                
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR PASS
        public void CD_ActualizarPass(CE_Usuarios usuario)
        {
            try
            {
                string sql = "UPDATE \"Usuarios\" SET \"contrasenia\" = pgp_sym_encrypt(@Contrasenia, @patron) WHERE \"IdUsuario\" = @IdUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@Contrasenia", usuario.Contrasenia);
                cmd.Parameters.AddWithValue("@patron", usuario.Patron);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR IMG
        public void CD_ActualizarIMG(CE_Usuarios usuario)
        {
            try
            {
                string sql = "UPDATE \"Usuarios\" SET \"img\" = @img WHERE \"IdUsuario\" = @IdUsuario";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@img", (object)usuario.Img ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region Buscar USUARIOS
        public DataTable Buscar(string buscar)
        {
            DataTable dt = new DataTable();
            try
            {
                // OPTIMIZACION: No traer columna 'img' en listado
                string sql = "SELECT u.\"IdUsuario\", u.\"Nombre\", u.\"Apellido\", u.\"DNI\", u.\"CUIT\", u.\"Correo\", u.\"Telefono\", u.\"Fecha_Nac\", u.\"Privilegio\", u.\"usuario\", u.\"Activo\", p.\"NombrePrivilegio\" " +
                             "FROM \"Usuarios\" u " +
                             "INNER JOIN \"Privilegios\" p ON u.\"Privilegio\" = p.\"IdPrivilegio\" " +
                             "WHERE u.\"Nombre\" ILIKE @buscar || '%' OR u.\"Apellido\" ILIKE @buscar || '%' OR u.\"usuario\" ILIKE @buscar || '%' " +
                             "ORDER BY u.\"Activo\" DESC";
                
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@buscar", buscar);
                da.Fill(dt);
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return dt;
        }
        #endregion

        // LOGIN
        #region LOGIN
        public CE_Usuarios Login(string usuario, string contra)
        {
            string patron = "PuntoDeVenta";
            try
            {
                // Llamamos a la función sp_u_validar definida en el script de Supabase
                string sql = "SELECT * FROM sp_u_validar(@Usuario, @Contra, @Patron)";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                da.SelectCommand.Parameters.AddWithValue("@Usuario", usuario);
                da.SelectCommand.Parameters.AddWithValue("@Contra", contra);
                da.SelectCommand.Parameters.AddWithValue("@Patron", patron);
                
                DataTable dt = new DataTable();
                da.Fill(dt);
                
                if(dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    this.capaEntidadUsuarios.IdUsuario = Convert.ToInt32(row["IdUsuario"]);
                    this.capaEntidadUsuarios.Privilegio = Convert.ToInt32(row["Privilegio"]);
                    this.capaEntidadUsuarios.Activo = Convert.ToBoolean(row["Activo"]);
                }
                else
                {
                    this.capaEntidadUsuarios.IdUsuario = 0;
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return this.capaEntidadUsuarios;
        }
        #endregion
    }
}