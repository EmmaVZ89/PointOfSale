using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace PuntoDeVenta.API.Security
{
    /// <summary>
    /// Middleware para agregar headers de seguridad HTTP
    /// OWASP A05: Security Misconfiguration - Headers correctamente configurados
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // =========================================
            // SECURITY HEADERS
            // =========================================

            // X-Content-Type-Options: Previene MIME type sniffing
            // El navegador no intentara adivinar el tipo de contenido
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // X-Frame-Options: Previene clickjacking
            // La pagina no puede ser embebida en iframes
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // X-XSS-Protection: Filtro XSS del navegador (legacy pero util)
            // mode=block: bloquea la pagina si detecta XSS
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // Referrer-Policy: Controla que informacion se envia en el header Referer
            // strict-origin-when-cross-origin: Solo envia origen en cross-origin
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Content-Security-Policy: Define origenes permitidos para recursos
            // Para API, restricciones mas estrictas
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'none'; " +
                "frame-ancestors 'none'; " +
                "form-action 'self'; " +
                "base-uri 'self'";

            // Permissions-Policy: Deshabilita APIs del navegador no necesarias
            context.Response.Headers["Permissions-Policy"] =
                "geolocation=(), microphone=(), camera=(), payment=(), usb=()";

            // Cache-Control: Para endpoints API, previene cache de datos sensibles
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }

            // Strict-Transport-Security: Fuerza HTTPS (solo en produccion)
            // Se aplica en el pipeline principal cuando ASPNETCORE_ENVIRONMENT=Production

            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods para registrar el middleware
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de Security Headers al pipeline
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
