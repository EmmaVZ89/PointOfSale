using System;

namespace Capa_Datos
{
    /// <summary>
    /// Helper centralizado para manejo de fechas con zona horaria Argentina.
    /// Soporta Windows (Argentina Standard Time) y Linux (America/Argentina/Buenos_Aires).
    /// </summary>
    public static class DateTimeHelper
    {
        private static TimeZoneInfo _argentinaTimeZone;
        private static readonly object _lock = new object();

        /// <summary>
        /// Zona horaria de Argentina (UTC-3).
        /// Compatible con Windows y Linux.
        /// </summary>
        public static TimeZoneInfo ArgentinaTimeZone
        {
            get
            {
                if (_argentinaTimeZone == null)
                {
                    lock (_lock)
                    {
                        if (_argentinaTimeZone == null)
                        {
                            try
                            {
                                // Windows
                                _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                            }
                            catch (TimeZoneNotFoundException)
                            {
                                try
                                {
                                    // Linux/macOS (IANA)
                                    _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
                                }
                                catch (TimeZoneNotFoundException)
                                {
                                    // Fallback: crear zona horaria manualmente (UTC-3)
                                    _argentinaTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                                        "Argentina",
                                        TimeSpan.FromHours(-3),
                                        "Argentina Standard Time",
                                        "Argentina Standard Time");
                                }
                            }
                        }
                    }
                }
                return _argentinaTimeZone;
            }
        }

        /// <summary>
        /// Obtiene la fecha y hora actual en Argentina.
        /// </summary>
        public static DateTime GetArgentinaNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);
        }

        /// <summary>
        /// Obtiene la fecha actual en Argentina (sin hora).
        /// </summary>
        public static DateTime GetArgentinaToday()
        {
            return GetArgentinaNow().Date;
        }

        /// <summary>
        /// Convierte una fecha UTC a hora Argentina.
        /// </summary>
        public static DateTime ConvertUtcToArgentina(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                utcDateTime = utcDateTime.ToUniversalTime();
            }
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
                ArgentinaTimeZone);
        }

        /// <summary>
        /// Convierte una fecha en hora Argentina a UTC.
        /// Usar cuando el cliente envía fechas en zona Argentina.
        /// </summary>
        public static DateTime ConvertArgentinaToUtc(DateTime argentinaDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(argentinaDateTime, DateTimeKind.Unspecified),
                ArgentinaTimeZone);
        }

        /// <summary>
        /// Obtiene el inicio del dia en Argentina convertido a UTC.
        /// Util para filtros de fecha.
        /// </summary>
        public static DateTime GetArgentinaDayStartUtc(DateTime argentinaDate)
        {
            var startOfDay = argentinaDate.Date;
            return ConvertArgentinaToUtc(startOfDay);
        }

        /// <summary>
        /// Obtiene el fin del dia en Argentina convertido a UTC.
        /// Util para filtros de fecha (23:59:59.999).
        /// </summary>
        public static DateTime GetArgentinaDayEndUtc(DateTime argentinaDate)
        {
            var endOfDay = argentinaDate.Date.AddDays(1).AddMilliseconds(-1);
            return ConvertArgentinaToUtc(endOfDay);
        }

        /// <summary>
        /// Formatea una fecha UTC para mostrar en Argentina.
        /// </summary>
        public static string FormatForDisplay(DateTime utcDateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            return ConvertUtcToArgentina(utcDateTime).ToString(format);
        }

        /// <summary>
        /// Formatea una fecha UTC para mostrar solo la fecha en Argentina.
        /// </summary>
        public static string FormatDateOnly(DateTime utcDateTime)
        {
            return ConvertUtcToArgentina(utcDateTime).ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Formatea una fecha UTC para mostrar fecha y hora en Argentina.
        /// </summary>
        public static string FormatDateTime(DateTime utcDateTime)
        {
            return ConvertUtcToArgentina(utcDateTime).ToString("dd/MM/yyyy HH:mm");
        }
    }
}
