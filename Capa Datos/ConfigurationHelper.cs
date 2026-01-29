using System;
using System.IO;
using System.Text.Json;

namespace Capa_Datos
{
    /// <summary>
    /// Clase helper para manejar la configuracion de la aplicacion.
    /// Prioridad de configuracion:
    /// 1. Variables de entorno (produccion/Railway)
    /// 2. appsettings.local.json (desarrollo local con credenciales)
    /// 3. appsettings.json (valores por defecto)
    /// </summary>
    public static class ConfigurationHelper
    {
        private static AppSettings _settings;
        private static string _connectionString;
        private static System.Collections.Generic.Dictionary<string, string> _additionalSettings;
        private static bool _isLoaded = false;

        /// <summary>
        /// Obtiene la cadena de conexion a la base de datos.
        /// Prioridad: DATABASE_URL (env) > appsettings.local.json > appsettings.json
        /// </summary>
        public static string GetConnectionString(string name = "DefaultConnection")
        {
            EnsureLoaded();

            // 1. Primero intentar variable de entorno DATABASE_URL (Railway)
            var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                return ConvertDatabaseUrlIfNeeded(envConnectionString);
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
                EnsureLoaded();
                return _settings;
            }
        }

        /// <summary>
        /// Obtiene un valor de configuracion por nombre.
        /// Prioridad: Variable de entorno > appsettings.local.json > appsettings.json
        /// </summary>
        public static string GetAppSetting(string key)
        {
            EnsureLoaded();

            // 1. Primero buscar en variables de entorno
            // Mapeo de nombres de config a variables de entorno
            var envKey = key switch
            {
                "JwtKey" => "JWT_KEY",
                "JwtIssuer" => "JWT_ISSUER",
                "JwtAudience" => "JWT_AUDIENCE",
                "JwtExpirationMinutes" => "JWT_EXPIRATION_MINUTES",
                "AllowedOrigins" => "ALLOWED_ORIGINS",
                _ => key.ToUpperInvariant().Replace(".", "_")
            };

            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // 2. Buscar en configuracion cargada del archivo
            if (_additionalSettings != null && _additionalSettings.TryGetValue(key, out string value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Convierte DATABASE_URL de formato URL a connection string si es necesario
        /// Railway usa: postgresql://user:password@host:port/database
        /// </summary>
        private static string ConvertDatabaseUrlIfNeeded(string databaseUrl)
        {
            if (string.IsNullOrEmpty(databaseUrl))
                return databaseUrl;

            // Si ya es un connection string (contiene "Host=" o "Server="), devolverlo
            if (databaseUrl.Contains("Host=") || databaseUrl.Contains("Server="))
                return databaseUrl;

            // Si es formato URL, convertir
            if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
            {
                try
                {
                    var uri = new Uri(databaseUrl);
                    var userInfo = uri.UserInfo.Split(':');
                    var user = userInfo[0];
                    var password = userInfo.Length > 1 ? userInfo[1] : "";
                    var host = uri.Host;
                    var port = uri.Port > 0 ? uri.Port : 5432;
                    var database = uri.AbsolutePath.TrimStart('/');

                    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
                }
                catch
                {
                    return databaseUrl;
                }
            }

            return databaseUrl;
        }

        /// <summary>
        /// Asegura que la configuracion este cargada
        /// </summary>
        private static void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                LoadConfiguration();
                _isLoaded = true;
            }
        }

        /// <summary>
        /// Carga la configuracion desde archivos
        /// </summary>
        private static void LoadConfiguration()
        {
            _settings = new AppSettings();
            _additionalSettings = new System.Collections.Generic.Dictionary<string, string>();

            // Intentar cargar appsettings.local.json primero (tiene prioridad)
            string localConfigPath = FindConfigurationFile("appsettings.local.json");
            if (!string.IsNullOrEmpty(localConfigPath))
            {
                LoadFromFile(localConfigPath);
                return;
            }

            // Si no existe local, cargar appsettings.json
            string configPath = FindConfigurationFile("appsettings.json");
            if (!string.IsNullOrEmpty(configPath))
            {
                LoadFromFile(configPath);
                return;
            }

            // Si no se encuentra ningun archivo, verificar si hay variables de entorno
            var envDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(envDbUrl))
            {
                // En produccion sin archivo de config, usar solo variables de entorno
                _connectionString = ConvertDatabaseUrlIfNeeded(envDbUrl);
                return;
            }

            // Solo lanzar error si no hay ninguna configuracion disponible
            throw new FileNotFoundException(
                "No se encontro configuracion. " +
                "Usa appsettings.json, appsettings.local.json, o configura DATABASE_URL en variables de entorno.");
        }

        /// <summary>
        /// Carga configuracion desde un archivo JSON
        /// </summary>
        private static void LoadFromFile(string configPath)
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);

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
                    if (root.TryGetProperty("AppSettings", out JsonElement appSettings))
                    {
                        if (appSettings.TryGetProperty("Timezone", out JsonElement tz))
                            _settings.Timezone = tz.GetString();
                        if (appSettings.TryGetProperty("AppName", out JsonElement appName))
                            _settings.AppName = appName.GetString();
                        if (appSettings.TryGetProperty("Version", out JsonElement version))
                            _settings.Version = version.GetString();

                        // Cargar settings adicionales
                        foreach (var prop in appSettings.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                _additionalSettings[prop.Name] = prop.Value.GetString();
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Error al parsear {configPath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca un archivo de configuracion en varias ubicaciones
        /// </summary>
        private static string FindConfigurationFile(string fileName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Buscar en el directorio del ejecutable
            string directPath = Path.Combine(basePath, fileName);
            if (File.Exists(directPath))
                return directPath;

            // 2. Subir por el arbol de directorios
            DirectoryInfo directory = new DirectoryInfo(basePath);

            while (directory != null && directory.Parent != null)
            {
                string configPath = Path.Combine(directory.FullName, fileName);
                if (File.Exists(configPath))
                    return configPath;

                // Si hay .sln, estamos en la raiz
                if (Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
                {
                    configPath = Path.Combine(directory.FullName, fileName);
                    if (File.Exists(configPath))
                        return configPath;
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
