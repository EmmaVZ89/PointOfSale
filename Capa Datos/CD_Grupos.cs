using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using Capa_Entidad;

namespace Capa_Datos
{
    public class CD_Grupos
    {
        CD_Conexion conn = new CD_Conexion();
        CE_Grupos cE_Grupos = new CE_Grupos();

        #region LISTAR GRUPOS
        public List<string> ListarGrupos()
        {
            List<string> retorno = new List<string>();
            try
            {
                string sql = "SELECT \"Nombre\" FROM \"Grupos\"";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        retorno.Add(dr["Nombre"].ToString());
                    }
                }
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return retorno;
        }
        #endregion

        #region NOMBRE GRUPO
        public CE_Grupos Nombre(int idGrupo)
        {
            try
            {
                string sql = "SELECT \"Nombre\" FROM \"Grupos\" WHERE \"IdGrupo\" = @IdGrupo";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@IdGrupo", idGrupo);
                object result = cmd.ExecuteScalar();
                if (result != null) this.cE_Grupos.Nombre = result.ToString();
            }
            finally
            {
                this.conn.CerrarConexion();
            }
            return this.cE_Grupos;
        }
        #endregion

        #region IDGRUPO
        public int IdGrupo(string nombre)
        {
            try
            {
                string sql = "SELECT \"IdGrupo\" FROM \"Grupos\" WHERE \"Nombre\" = @Nombre";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn.AbrirConexion());
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            finally
            {
                this.conn.CerrarConexion();
            }
        }
        #endregion
    }
}