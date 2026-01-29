using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PuntoDeVenta.API.Security
{
    /// <summary>
    /// Tipos de eventos de auditoria
    /// </summary>
    public enum AuditEventType
    {
        // Autenticacion
        LoginSuccess,
        LoginFailed,
        LoginLocked,
        Logout,
        TokenRefresh,

        // Autorizacion
        AccessDenied,
        UnauthorizedAccess,

        // Datos
        DataCreated,
        DataUpdated,
        DataDeleted,
        DataExported,

        // Seguridad
        RateLimitExceeded,
        SuspiciousActivity,
        SecurityException
    }

    /// <summary>
    /// Interfaz para logging de auditoria
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Registra un evento de autenticacion
        /// </summary>
        void LogAuthentication(AuditEventType eventType, string username, string ipAddress, string details = null);

        /// <summary>
        /// Registra un evento de autorizacion
        /// </summary>
        void LogAuthorization(AuditEventType eventType, string userId, string resource, string action, string ipAddress);

        /// <summary>
        /// Registra un evento de datos (CRUD)
        /// </summary>
        void LogData(AuditEventType eventType, string userId, string entity, string entityId, string details = null);

        /// <summary>
        /// Registra un evento de seguridad
        /// </summary>
        void LogSecurity(AuditEventType eventType, string ipAddress, string details);
    }

    /// <summary>
    /// Implementacion de logging de auditoria usando Serilog
    /// OWASP A09: Security Logging and Monitoring Failures
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(ILogger<AuditLogger> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Registra eventos de autenticacion (login, logout, etc.)
        /// </summary>
        public void LogAuthentication(AuditEventType eventType, string username, string ipAddress, string details = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var userAgent = context?.Request.Headers["User-Agent"].ToString();

            // Usar structured logging para facilitar busquedas
            _logger.LogInformation(
                "AUDIT:AUTH | Event: {EventType} | User: {Username} | IP: {IpAddress} | UserAgent: {UserAgent} | Details: {Details}",
                eventType.ToString(),
                SanitizeForLog(username),
                ipAddress,
                userAgent,
                details ?? "N/A");
        }

        /// <summary>
        /// Registra eventos de autorizacion (acceso denegado, etc.)
        /// </summary>
        public void LogAuthorization(AuditEventType eventType, string userId, string resource, string action, string ipAddress)
        {
            _logger.LogWarning(
                "AUDIT:AUTHZ | Event: {EventType} | UserId: {UserId} | Resource: {Resource} | Action: {Action} | IP: {IpAddress}",
                eventType.ToString(),
                userId ?? "anonymous",
                SanitizeForLog(resource),
                action,
                ipAddress);
        }

        /// <summary>
        /// Registra eventos de datos (CRUD operations)
        /// </summary>
        public void LogData(AuditEventType eventType, string userId, string entity, string entityId, string details = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(context);

            _logger.LogInformation(
                "AUDIT:DATA | Event: {EventType} | UserId: {UserId} | Entity: {Entity} | EntityId: {EntityId} | IP: {IpAddress} | Details: {Details}",
                eventType.ToString(),
                userId ?? "system",
                entity,
                entityId,
                ipAddress,
                details ?? "N/A");
        }

        /// <summary>
        /// Registra eventos de seguridad (rate limiting, actividad sospechosa, etc.)
        /// </summary>
        public void LogSecurity(AuditEventType eventType, string ipAddress, string details)
        {
            _logger.LogWarning(
                "AUDIT:SECURITY | Event: {EventType} | IP: {IpAddress} | Details: {Details} | Timestamp: {Timestamp}",
                eventType.ToString(),
                ipAddress,
                SanitizeForLog(details),
                DateTime.UtcNow.ToString("O"));
        }

        /// <summary>
        /// Sanitiza texto para prevenir log injection
        /// </summary>
        private string SanitizeForLog(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remover caracteres que podrian usarse para log injection
            return input
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ")
                .Replace("|", "-");
        }

        /// <summary>
        /// Obtiene la IP del cliente considerando proxies
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            if (context == null)
                return "unknown";

            // X-Forwarded-For para obtener IP real detras de proxy/load balancer
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
