using System;
using System.Collections.Generic;
using Npgsql;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Clientes
    {
        private readonly CD_Conexion conn = new CD_Conexion();
        private readonly CE_Clientes capaEntidadClientes = new CE_Clientes();

        #region INSERTAR
        public void CD_Insertar(CE_Clientes cliente)
        {
            try
            {
                string sql = "INSERT INTO \"Clientes\" (\"RazonSocial\", \"Documento\", \"Telefono\", \"Email\", \"Domicilio\", \"Activo\") " +
                             "VALUES (@RazonSocial, @Documento, @Telefono, @Email, @Domicilio, TRUE)";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@RazonSocial", cliente.RazonSocial);
                cmd.Parameters.AddWithValue("@Documento", (object)cliente.Documento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Domicilio", (object)cliente.Domicilio ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region CONSULTAR
        public CE_Clientes CD_Consultar(int idCliente)
        {
            string sql = "SELECT \"IdCliente\", \"RazonSocial\", \"Documento\", \"Telefono\", \"Email\", \"Domicilio\", \"Activo\", \"FechaAlta\" " +
                         "FROM \"Clientes\" WHERE \"IdCliente\" = @IdCliente";

            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
            adapter.SelectCommand.Parameters.AddWithValue("@IdCliente", idCliente);

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                this.capaEntidadClientes.IdCliente = Convert.ToInt32(row["IdCliente"]);
                this.capaEntidadClientes.RazonSocial = Convert.ToString(row["RazonSocial"]);
                this.capaEntidadClientes.Documento = row["Documento"] != DBNull.Value ? Convert.ToString(row["Documento"]) : "";
                this.capaEntidadClientes.Telefono = row["Telefono"] != DBNull.Value ? Convert.ToString(row["Telefono"]) : "";
                this.capaEntidadClientes.Email = row["Email"] != DBNull.Value ? Convert.ToString(row["Email"]) : "";
                this.capaEntidadClientes.Domicilio = row["Domicilio"] != DBNull.Value ? Convert.ToString(row["Domicilio"]) : "";
                this.capaEntidadClientes.Activo = Convert.ToBoolean(row["Activo"]);
                this.capaEntidadClientes.FechaAlta = Convert.ToDateTime(row["FechaAlta"]);
            }

            return this.capaEntidadClientes;
        }
        #endregion

        #region REACTIVAR
        public void CD_Reactivar(CE_Clientes cliente)
        {
            try
            {
                string sql = "UPDATE \"Clientes\" SET \"Activo\" = TRUE WHERE \"IdCliente\" = @IdCliente";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ELIMINAR
        public void CD_Eliminar(CE_Clientes cliente)
        {
            try
            {
                string sql = "UPDATE \"Clientes\" SET \"Activo\" = FALSE WHERE \"IdCliente\" = @IdCliente";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region ACTUALIZAR DATOS
        public void CD_ActualizarDatos(CE_Clientes cliente)
        {
            try
            {
                string sql = "UPDATE \"Clientes\" SET \"RazonSocial\"=@RazonSocial, \"Documento\"=@Documento, " +
                             "\"Telefono\"=@Telefono, \"Email\"=@Email, \"Domicilio\"=@Domicilio WHERE \"IdCliente\" = @IdCliente";

                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                cmd.Parameters.AddWithValue("@RazonSocial", cliente.RazonSocial);
                cmd.Parameters.AddWithValue("@Documento", (object)cliente.Documento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Domicilio", (object)cliente.Domicilio ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region BUSCAR CLIENTES
        public DataTable Buscar(string buscar)
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT \"IdCliente\", \"RazonSocial\", \"Documento\", \"Telefono\", \"Email\", \"Domicilio\", \"Activo\", \"FechaAlta\" " +
                             "FROM \"Clientes\" " +
                             "WHERE \"RazonSocial\" ILIKE '%' || @buscar || '%' OR \"Documento\" ILIKE '%' || @buscar || '%' " +
                             "ORDER BY \"Activo\" DESC, \"RazonSocial\" ASC";

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

        #region LISTAR ACTIVOS (Para ComboBox)
        public List<CE_Clientes> ListarActivos()
        {
            List<CE_Clientes> lista = new List<CE_Clientes>();
            try
            {
                string sql = "SELECT \"IdCliente\", \"RazonSocial\", \"Documento\", \"Domicilio\" " +
                             "FROM \"Clientes\" " +
                             "WHERE \"Activo\" = TRUE " +
                             "ORDER BY \"IdCliente\" ASC";

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, this.conn.AbrirConexion());
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    CE_Clientes cliente = new CE_Clientes
                    {
                        IdCliente = Convert.ToInt32(row["IdCliente"]),
                        RazonSocial = Convert.ToString(row["RazonSocial"]),
                        Documento = row["Documento"] != DBNull.Value ? Convert.ToString(row["Documento"]) : "",
                        Domicilio = row["Domicilio"] != DBNull.Value ? Convert.ToString(row["Domicilio"]) : ""
                    };
                    lista.Add(cliente);
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return lista;
        }
        #endregion
    }
}
