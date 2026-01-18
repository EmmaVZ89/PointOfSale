using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Privilegios
    {
        readonly CD_Conexion conn = new CD_Conexion();
        readonly CE_Privilegios cePrivilegios = new CE_Privilegios();

        #region IDPRIVILEGIO
        public int IdPrivilegio(string nombrePrivilegio)
        {
            try
            {
                string sql = "SELECT \"IdPrivilegio\" FROM \"Privilegios\" WHERE \"NombrePrivilegio\" = @Nombre";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Nombre", nombrePrivilegio);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion

        #region NOMBREPRIVILEGIO
        public CE_Privilegios NombrePrivilegio(int idPrivilegio)
        {
            try
            {
                string sql = "SELECT \"NombrePrivilegio\" FROM \"Privilegios\" WHERE \"IdPrivilegio\" = @Id";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Id", idPrivilegio);
                object result = cmd.ExecuteScalar();
                if (result != null) this.cePrivilegios.NombrePrivilegio = result.ToString();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return this.cePrivilegios;
        }
        #endregion

        #region LISTAR PRIVILEGIOS
        public List<string> ObtenerPrivilegios()
        {
            List<string> lista = new List<string>();
            try
            {
                string sql = "SELECT \"NombrePrivilegio\" FROM \"Privilegios\"";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(dr["NombrePrivilegio"].ToString());
                    }
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