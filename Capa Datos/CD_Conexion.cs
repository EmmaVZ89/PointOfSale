using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;

namespace Capa_Datos
{
    /// <summary>
    /// Clase que maneja la conexion a la base de datos PostgreSQL.
    ///
    /// CAMBIO DE SEGURIDAD: La cadena de conexion ahora se lee desde appsettings.json
    /// en lugar de estar hardcodeada en el codigo.
    ///
    /// Esto permite:
    /// - No exponer credenciales en el repositorio de codigo
    /// - Tener diferentes configuraciones por ambiente (desarrollo, produccion)
    /// - Cambiar la conexion sin recompilar la aplicacion
    /// </summary>
    public class CD_Conexion
    {
        // La cadena de conexion ahora se obtiene del archivo de configuracion
        private readonly string connectionString;

        private readonly NpgsqlConnection conn;

        public CD_Conexion()
        {
            // Obtener la cadena de conexion desde appsettings.json
            // usando el helper de configuracion
            this.connectionString = ConfigurationHelper.GetConnectionString("DefaultConnection");

            // Validar que la cadena de conexion no este vacia
            if (string.IsNullOrEmpty(this.connectionString))
            {
                throw new InvalidOperationException(
                    "No se encontro la cadena de conexion 'DefaultConnection' en appsettings.json. " +
                    "Verifica que el archivo existe y tiene la configuracion correcta.");
            }

            this.conn = new NpgsqlConnection(this.connectionString);
        }

        public NpgsqlConnection AbrirConexion()
        {
            if(this.conn.State == ConnectionState.Closed)
            {
                this.conn.Open();
            }
            return this.conn;
        }

        public NpgsqlConnection CerrarConexion()
        {
            if(this.conn.State == ConnectionState.Open)
            {
                this.conn.Close();
            }
            return this.conn;
        }
    }
}
