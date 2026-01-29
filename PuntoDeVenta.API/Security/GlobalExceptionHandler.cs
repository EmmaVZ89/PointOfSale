using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PuntoDeVenta.API.Security
{
    /// <summary>
    /// Manejador global de excepciones que sanitiza errores para produccion
    /// OWASP A09: Security Logging and Monitoring Failures - Log completo interno
    /// Previene exposicion de stack traces y detalles internos al cliente
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IAuditLogger _auditLogger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IAuditLogger auditLogger)
        {
            _logger = logger;
            _auditLogger = auditLogger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Obtener IP del cliente
            var ipAddress = GetClientIpAddress(httpContext);
            var requestPath = httpContext.Request.Path;
            var requestMethod = httpContext.Request.Method;
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            // LOG INTERNO COMPLETO (para debugging)
            _logger.LogError(
                exception,
                "Unhandled exception | CorrelationId: {CorrelationId} | Method: {Method} | Path: {Path} | IP: {IpAddress} | Message: {Message}",
                correlationId,
                requestMethod,
                requestPath,
                ipAddress,
                exception.Message);

            // Registrar en audit log
            _auditLogger.LogSecurity(
                AuditEventType.SecurityException,
                ipAddress,
                $"CorrelationId: {correlationId}, Path: {requestPath}");

            // Determinar codigo de estado basado en el tipo de excepcion
            var (statusCode, userMessage) = MapExceptionToResponse(exception);

            // RESPUESTA SANITIZADA AL CLIENTE (nunca exponer detalles internos)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetStatusCodeTitle(statusCode),
                Detail = userMessage,
                Instance = requestPath,
                Extensions =
                {
                    ["correlationId"] = correlationId,
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                }
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails, jsonOptions),
                cancellationToken);

            return true; // Excepcion manejada
        }

        /// <summary>
        /// Mapea tipos de excepcion a codigos de estado y mensajes apropiados
        /// </summary>
        private (int StatusCode, string Message) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                // Excepciones de validacion
                ArgumentException or ArgumentNullException =>
                    ((int)HttpStatusCode.BadRequest, "Los datos proporcionados no son válidos."),

                // Excepciones de autorizacion
                UnauthorizedAccessException =>
                    ((int)HttpStatusCode.Unauthorized, "No tiene autorización para realizar esta operación."),

                // Excepciones de no encontrado
                KeyNotFoundException or InvalidOperationException when exception.Message.Contains("not found") =>
                    ((int)HttpStatusCode.NotFound, "El recurso solicitado no fue encontrado."),

                // Timeout
                TimeoutException or TaskCanceledException =>
                    ((int)HttpStatusCode.RequestTimeout, "La operación tardó demasiado tiempo. Por favor, intente nuevamente."),

                // Cualquier otra excepcion
                _ => ((int)HttpStatusCode.InternalServerError, "Ocurrió un error interno. Por favor, contacte al administrador si el problema persiste.")
            };
        }

        /// <summary>
        /// Obtiene el titulo estandar para un codigo de estado HTTP
        /// </summary>
        private string GetStatusCodeTitle(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                408 => "Request Timeout",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                503 => "Service Unavailable",
                _ => "Error"
            };
        }

        /// <summary>
        /// Obtiene la IP del cliente considerando proxies
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
