using System;
using System.IO;
using System.Text.Json;

namespace Capa_Datos
{
    /// <summary>
    /// Clase helper para manejar la configuracion de la aplicacion.
    /// Lee los valores desde appsettings.json ubicado en la raiz del proyecto.
    ///
    /// NOTA: Usa System.Text.Json (ya incluido en el proyecto) en lugar de
    /// Microsoft.Extensions.Configuration para evitar problemas de dependencias
    /// con el formato antiguo de packages.config del proyecto WPF.
    /// </summary>
    public static class ConfigurationHelper
    {
        // Instancia estatica de la configuracion (patron Singleton)
        private static AppSettings _settings;
        private static string _connectionString;

        /// <summary>
        /// Obtiene la cadena de conexion a la base de datos.
        /// </summary>
        public static string GetConnectionString(string name = "DefaultConnection")
        {
            if (_connectionString == null)
            {
                LoadConfiguration();
            }
            return _connectionString;
        }

        /// <summary>
        /// Obtiene la configuracion de la aplicacion.
        /// </summary>
        public static AppSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    LoadConfiguration();
                }
                return _settings;
            }
        }

        /// <summary>
        /// Obtiene un valor de configuracion por nombre.
        /// Util para configuraciones dinamicas como JWT.
        /// </summary>
        /// <param name="key">Nombre de la configuracion (ej: "JwtKey")</param>
        /// <returns>Valor de la configuracion o null si no existe</returns>
        public static string GetAppSetting(string key)
        {
            if (_settings == null)
            {
                LoadConfiguration();
            }

            // Buscar en el diccionario de settings adicionales
            if (_additionalSettings != null && _additionalSettings.TryGetValue(key, out string value))
            {
                return value;
            }

            return null;
        }

        // Diccionario para settings adicionales (JWT, etc.)
        private static System.Collections.Generic.Dictionary<string, string> _additionalSettings;

        /// <summary>
        /// Carga la configuracion desde appsettings.json
        /// </summary>
        private static void LoadConfiguration()
        {
            string configPath = FindConfigurationFile();

            if (string.IsNullOrEmpty(configPath))
            {
                throw new FileNotFoundException(
                    "No se encontro el archivo appsettings.json. " +
                    "Asegurate de copiarlo a la carpeta bin/Debug o a la raiz del proyecto.");
            }

            try
            {
                string jsonContent = File.ReadAllText(configPath);

                // Parsear el JSON
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    // Leer ConnectionStrings
                    if (root.TryGetProperty("ConnectionStrings", out JsonElement connStrings))
                    {
                        if (connStrings.TryGetProperty("DefaultConnection", out JsonElement defaultConn))
                        {
                            _connectionString = defaultConn.GetString();
                        }
                    }

                    // Leer AppSettings
                    _settings = new AppSettings();
                    _additionalSettings = new System.Collections.Generic.Dictionary<string, string>();

                    if (root.TryGetProperty("AppSettings", out JsonElement appSettings))
                    {
                        if (appSettings.TryGetProperty("Timezone", out JsonElement tz))
                        {
                            _settings.Timezone = tz.GetString();
                        }
                        if (appSettings.TryGetProperty("AppName", out JsonElement appName))
                        {
                            _settings.AppName = appName.GetString();
                        }
                        if (appSettings.TryGetProperty("Version", out JsonElement version))
                        {
                            _settings.Version = version.GetString();
                        }

                        // Cargar settings adicionales (JWT, etc.)
                        if (appSettings.TryGetProperty("JwtKey", out JsonElement jwtKey))
                        {
                            _additionalSettings["JwtKey"] = jwtKey.GetString();
                        }
                        if (appSettings.TryGetProperty("JwtIssuer", out JsonElement jwtIssuer))
                        {
                            _additionalSettings["JwtIssuer"] = jwtIssuer.GetString();
                        }
                        if (appSettings.TryGetProperty("JwtAudience", out JsonElement jwtAudience))
                        {
                            _additionalSettings["JwtAudience"] = jwtAudience.GetString();
                        }
                        if (appSettings.TryGetProperty("JwtExpirationMinutes", out JsonElement jwtExp))
                        {
                            _additionalSettings["JwtExpirationMinutes"] = jwtExp.GetString();
                        }
                    }
                }

                // Validar que se leyo la cadena de conexion
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException(
                        "No se encontro 'ConnectionStrings:DefaultConnection' en appsettings.json");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Error al parsear appsettings.json: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca el archivo appsettings.json en varias ubicaciones
        /// </summary>
        private static string FindConfigurationFile()
        {
            const string fileName = "appsettings.json";
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Buscar en el directorio del ejecutable (produccion/debug)
            string directPath = Path.Combine(basePath, fileName);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            // 2. Subir por el arbol de directorios buscando el archivo
            DirectoryInfo directory = new DirectoryInfo(basePath);

            while (directory != null && directory.Parent != null)
            {
                string configPath = Path.Combine(directory.FullName, fileName);
                if (File.Exists(configPath))
                {
                    return configPath;
                }

                // Buscar si hay un archivo .sln (indica raiz del proyecto)
                if (Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
                {
                    configPath = Path.Combine(directory.FullName, fileName);
                    if (File.Exists(configPath))
                    {
                        return configPath;
                    }
                    // Si estamos en la raiz y no lo encontramos, parar
                    break;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Clase para almacenar la configuracion de la aplicacion
    /// </summary>
    public class AppSettings
    {
        public string Timezone { get; set; } = "America/Argentina/Buenos_Aires";
        public string AppName { get; set; } = "PuntoDeVenta";
        public string Version { get; set; } = "1.0.0";
    }
}
