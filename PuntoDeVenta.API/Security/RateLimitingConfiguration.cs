using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace PuntoDeVenta.API.Security
{
    /// <summary>
    /// Configuracion de Rate Limiting para proteccion contra ataques de fuerza bruta y DoS
    /// OWASP A04: Insecure Design - Rate limiting previene abuso de endpoints
    /// OWASP A07: Auth Failures - Proteccion especial para endpoints de login
    /// </summary>
    public static class RateLimitingConfiguration
    {
        // Nombres de las politicas
        public const string GlobalPolicy = "global";
        public const string LoginPolicy = "login";
        public const string ApiPolicy = "api";

        /// <summary>
        /// Registra los servicios de Rate Limiting con politicas predefinidas
        /// </summary>
        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Callback cuando se rechaza una solicitud por rate limit
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = GetRetryAfterSeconds(context.Lease);
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                    var response = new
                    {
                        success = false,
                        message = "Demasiadas solicitudes. Por favor, espere antes de intentar nuevamente.",
                        retryAfterSeconds = retryAfter
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
                };

                // Politica GLOBAL: 100 requests por minuto por IP
                options.AddPolicy(GlobalPolicy, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIpAddress(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        }));

                // Politica LOGIN: 5 intentos por minuto por IP (proteccion brute force)
                options.AddPolicy(LoginPolicy, context =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetClientIpAddress(context),
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // No queue para login
                        }));

                // Politica API: 60 requests por minuto por IP (endpoints generales)
                options.AddPolicy(ApiPolicy, context =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: GetClientIpAddress(context),
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 60,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        }));
            });

            return services;
        }

        /// <summary>
        /// Obtiene la IP del cliente considerando proxies
        /// </summary>
        private static string GetClientIpAddress(HttpContext context)
        {
            // Primero intentar obtener IP del header X-Forwarded-For (Railway usa esto)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For puede contener multiples IPs, la primera es el cliente original
                var firstIp = forwardedFor.Split(',')[0].Trim();
                return firstIp;
            }

            // Fallback a la IP de conexion directa
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Calcula los segundos de espera para el header Retry-After
        /// </summary>
        private static int GetRetryAfterSeconds(RateLimitLease lease)
        {
            if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                return (int)retryAfter.TotalSeconds;
            }
            return 60; // Default: 1 minuto
        }
    }
}
