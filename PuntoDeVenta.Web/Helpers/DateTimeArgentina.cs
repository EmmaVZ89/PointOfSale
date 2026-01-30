using System;

namespace PuntoDeVenta.Web.Helpers
{
    /// <summary>
    /// Helper para obtener fecha y hora en zona horaria Argentina (UTC-3).
    /// Para uso en Blazor WASM donde TimeZoneInfo puede tener limitaciones.
    /// </summary>
    public static class DateTimeArgentina
    {
        /// <summary>
        /// Offset de Argentina respecto a UTC (UTC-3)
        /// </summary>
        private static readonly TimeSpan ArgentinaOffset = TimeSpan.FromHours(-3);

        /// <summary>
        /// Obtiene la fecha y hora actual en Argentina.
        /// </summary>
        public static DateTime Now => DateTime.UtcNow.Add(ArgentinaOffset);

        /// <summary>
        /// Obtiene la fecha actual en Argentina (sin hora).
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Obtiene el anio actual en Argentina.
        /// </summary>
        public static int Year => Now.Year;

        /// <summary>
        /// Convierte una fecha UTC a hora Argentina.
        /// </summary>
        public static DateTime FromUtc(DateTime utcDateTime)
        {
            return utcDateTime.Add(ArgentinaOffset);
        }

        /// <summary>
        /// Convierte una fecha en hora Argentina a UTC.
        /// </summary>
        public static DateTime ToUtc(DateTime argentinaDateTime)
        {
            return argentinaDateTime.Subtract(ArgentinaOffset);
        }

        /// <summary>
        /// Formatea la fecha actual en formato largo espanol.
        /// Ejemplo: "miércoles, 29 enero 2026"
        /// </summary>
        public static string FormatLongDate()
        {
            return Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
        }

        /// <summary>
        /// Formatea una fecha en formato dd/MM/yyyy.
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Formatea una fecha y hora en formato dd/MM/yyyy HH:mm.
        /// </summary>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Formatea una fecha y hora en formato dd/MM/yyyy HH:mm:ss.
        /// </summary>
        public static string FormatDateTimeFull(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        /// <summary>
        /// Genera un timestamp para nombres de archivo.
        /// Formato: yyyyMMddHHmmss
        /// </summary>
        public static string FileTimestamp => Now.ToString("yyyyMMddHHmmss");

        /// <summary>
        /// Genera un timestamp corto para nombres de archivo.
        /// Formato: yyyyMMdd
        /// </summary>
        public static string FileDate => Now.ToString("yyyyMMdd");
    }
}
