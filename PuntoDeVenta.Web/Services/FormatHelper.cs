using System.Globalization;

namespace PuntoDeVenta.Web.Services
{
    /// <summary>
    /// Helper estatico para formateo de numeros y moneda con cultura argentina (es-AR)
    /// Separador de miles: . (punto)
    /// Separador de decimales: , (coma)
    /// Simbolo de moneda: $ (sin ARS)
    /// </summary>
    public static class FormatHelper
    {
        // Cultura argentina con formato personalizado
        private static readonly CultureInfo ArgentinaCulture;
        private static readonly NumberFormatInfo CurrencyFormat;

        static FormatHelper()
        {
            // Crear cultura argentina
            ArgentinaCulture = new CultureInfo("es-AR");

            // Crear formato de moneda personalizado (solo $, sin ARS)
            CurrencyFormat = (NumberFormatInfo)ArgentinaCulture.NumberFormat.Clone();
            CurrencyFormat.CurrencySymbol = "$";
            CurrencyFormat.CurrencyDecimalSeparator = ",";
            CurrencyFormat.CurrencyGroupSeparator = ".";
            CurrencyFormat.NumberDecimalSeparator = ",";
            CurrencyFormat.NumberGroupSeparator = ".";
        }

        /// <summary>
        /// Formatea un valor como moneda argentina (ej: $1.234,56)
        /// </summary>
        /// <param name="value">Valor decimal a formatear</param>
        /// <param name="decimals">Cantidad de decimales (default: 2)</param>
        /// <returns>String formateado como moneda</returns>
        public static string FormatCurrency(decimal value, int decimals = 2)
        {
            return value.ToString($"C{decimals}", CurrencyFormat);
        }

        /// <summary>
        /// Formatea un valor como moneda argentina sin decimales (ej: $1.234)
        /// </summary>
        public static string FormatCurrencyNoDecimals(decimal value)
        {
            return value.ToString("C0", CurrencyFormat);
        }

        /// <summary>
        /// Formatea un numero con separadores de miles (ej: 1.234,56)
        /// </summary>
        /// <param name="value">Valor decimal a formatear</param>
        /// <param name="decimals">Cantidad de decimales (default: 2)</param>
        /// <returns>String formateado</returns>
        public static string FormatNumber(decimal value, int decimals = 2)
        {
            return value.ToString($"N{decimals}", CurrencyFormat);
        }

        /// <summary>
        /// Formatea un numero entero con separadores de miles (ej: 1.234)
        /// </summary>
        public static string FormatInteger(decimal value)
        {
            return value.ToString("N0", CurrencyFormat);
        }

        /// <summary>
        /// Formatea un numero entero con separadores de miles (ej: 1.234)
        /// </summary>
        public static string FormatInteger(int value)
        {
            return value.ToString("N0", CurrencyFormat);
        }

        /// <summary>
        /// Formatea un porcentaje (ej: 15,50%)
        /// </summary>
        public static string FormatPercentage(decimal value, int decimals = 2)
        {
            return value.ToString($"P{decimals}", CurrencyFormat);
        }

        /// <summary>
        /// Formatea una fecha corta (ej: 21/01/2026)
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy", ArgentinaCulture);
        }

        /// <summary>
        /// Formatea una fecha con hora (ej: 21/01/2026 14:30)
        /// </summary>
        public static string FormatDateTime(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm", ArgentinaCulture);
        }

        /// <summary>
        /// Obtiene la cultura argentina para uso en formatos personalizados
        /// </summary>
        public static CultureInfo Culture => ArgentinaCulture;
    }
}
