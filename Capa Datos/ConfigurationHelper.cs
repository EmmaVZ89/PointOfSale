using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Capa_Datos
{
    /// <summary>
    /// Clase helper para manejar la configuracion de la aplicacion.
    /// Lee los valores desde appsettings.json ubicado en la raiz del proyecto.
    ///
    /// TECNOLOGIA NUEVA: Microsoft.Extensions.Configuration
    /// - Es el sistema de configuracion moderno de .NET
    /// - Reemplaza el viejo ConfigurationManager de System.Configuration
    /// - Soporta multiples fuentes: JSON, XML, variables de entorno, etc.
    /// - Es el estandar en ASP.NET Core y .NET Core
    /// </summary>
    public static class ConfigurationHelper
    {
        // Instancia estatica de la configuracion (patron Singleton)
        private static IConfiguration _configuration;

        /// <summary>
        /// Propiedad que carga y cachea la configuracion.
        /// Solo se inicializa una vez (lazy loading).
        /// </summary>
        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = LoadConfiguration();
                }
                return _configuration;
            }
        }

        /// <summary>
        /// Carga la configuracion desde appsettings.json
        /// </summary>
        private static IConfiguration LoadConfiguration()
        {
            // Obtener la ruta del ejecutable
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Buscar appsettings.json en varias ubicaciones posibles
            string configPath = FindConfigurationFile(basePath);

            if (string.IsNullOrEmpty(configPath))
            {
                throw new FileNotFoundException(
                    "No se encontro el archivo appsettings.json. " +
                    "Asegurate de que existe en la carpeta de la aplicacion o en la raiz del proyecto.");
            }

            // ConfigurationBuilder es el patron Builder aplicado
            // Permite construir la configuracion paso a paso
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath))
                .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: true);

            return builder.Build();
        }

        /// <summary>
        /// Busca el archivo appsettings.json subiendo por el arbol de directorios
        /// hasta encontrarlo o llegar a la raiz del disco.
        /// </summary>
        private static string FindConfigurationFile(string basePath)
        {
            const string fileName = "appsettings.json";

            // 1. Primero buscar en el directorio del ejecutable (produccion)
            string directPath = Path.Combine(basePath, fileName);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            // 2. Subir por el arbol de directorios buscando el archivo
            // Esto funciona en desarrollo cuando el exe esta en bin/Debug/net472/
            DirectoryInfo directory = new DirectoryInfo(basePath);

            while (directory != null && directory.Parent != null)
            {
                string configPath = Path.Combine(directory.FullName, fileName);
                if (File.Exists(configPath))
                {
                    return configPath;
                }

                // Tambien buscar si hay un archivo .sln (indica raiz del proyecto)
                string[] slnFiles = Directory.GetFiles(directory.FullName, "*.sln");
                if (slnFiles.Length > 0)
                {
                    // Estamos en la raiz del proyecto, el archivo deberia estar aqui
                    configPath = Path.Combine(directory.FullName, fileName);
                    if (File.Exists(configPath))
                    {
                        return configPath;
                    }
                }

                directory = directory.Parent;
            }

            return null;
        }

        /// <summary>
        /// Obtiene la cadena de conexion a la base de datos.
        /// </summary>
        public static string GetConnectionString(string name = "DefaultConnection")
        {
            // GetConnectionString es un metodo de extension que busca en la seccion "ConnectionStrings"
            return Configuration.GetConnectionString(name);
        }

        /// <summary>
        /// Obtiene un valor de configuracion por su clave.
        /// Ejemplo: GetValue("AppSettings:Timezone")
        /// </summary>
        public static string GetValue(string key)
        {
            // La sintaxis con ":" permite navegar secciones anidadas
            // "AppSettings:Timezone" busca { "AppSettings": { "Timezone": "valor" } }
            return Configuration[key];
        }

        /// <summary>
        /// Obtiene un valor de configuracion tipado.
        /// Ejemplo: GetValue<int>("AppSettings:MaxRetries")
        /// </summary>
        public static T GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        /// <summary>
        /// Obtiene una seccion completa de configuracion.
        /// Util para bindear a una clase fuertemente tipada.
        /// </summary>
        public static IConfigurationSection GetSection(string sectionName)
        {
            return Configuration.GetSection(sectionName);
        }
    }
}
