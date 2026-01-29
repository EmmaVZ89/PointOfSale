using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.API.Security
{
    /// <summary>
    /// Resultado de verificacion de intento de login
    /// </summary>
    public class LoginAttemptResult
    {
        public bool IsLocked { get; set; }
        public int RemainingAttempts { get; set; }
        public DateTime? LockoutEndsAt { get; set; }
        public int TotalAttempts { get; set; }
    }

    /// <summary>
    /// Interfaz para tracking de intentos de login
    /// </summary>
    public interface ILoginAttemptTracker
    {
        /// <summary>
        /// Verifica si el usuario/IP esta bloqueado
        /// </summary>
        LoginAttemptResult CheckAttempt(string username, string ipAddress);

        /// <summary>
        /// Registra un intento fallido
        /// </summary>
        void RecordFailedAttempt(string username, string ipAddress);

        /// <summary>
        /// Resetea los intentos despues de login exitoso
        /// </summary>
        void ResetAttempts(string username, string ipAddress);
    }

    /// <summary>
    /// Implementacion de tracking de intentos de login con proteccion anti brute-force
    /// OWASP A07: Identification and Authentication Failures
    /// </summary>
    public class LoginAttemptTracker : ILoginAttemptTracker
    {
        // Configuracion
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(30);

        // Almacenamiento en memoria (en produccion considerar Redis)
        private readonly ConcurrentDictionary<string, List<DateTime>> _failedAttempts = new();
        private readonly ConcurrentDictionary<string, DateTime> _lockouts = new();

        /// <summary>
        /// Genera una clave unica combinando username e IP
        /// Esto previene ataques distribuidos y por usuario
        /// </summary>
        private string GetKey(string username, string ipAddress)
        {
            // Combinar usuario e IP para tracking mas preciso
            var normalizedUsername = username?.ToLowerInvariant() ?? "unknown";
            return $"{normalizedUsername}|{ipAddress}";
        }

        /// <summary>
        /// Verifica si el usuario/IP esta bloqueado y retorna informacion del estado
        /// </summary>
        public LoginAttemptResult CheckAttempt(string username, string ipAddress)
        {
            var key = GetKey(username, ipAddress);
            CleanupOldAttempts(key);

            // Verificar si hay lockout activo
            if (_lockouts.TryGetValue(key, out var lockoutEnd) && lockoutEnd > DateTime.UtcNow)
            {
                return new LoginAttemptResult
                {
                    IsLocked = true,
                    RemainingAttempts = 0,
                    LockoutEndsAt = lockoutEnd,
                    TotalAttempts = MaxFailedAttempts
                };
            }

            // Remover lockout expirado
            if (lockoutEnd <= DateTime.UtcNow)
            {
                _lockouts.TryRemove(key, out _);
            }

            // Contar intentos actuales
            var attempts = GetRecentAttempts(key);
            var remainingAttempts = MaxFailedAttempts - attempts;

            return new LoginAttemptResult
            {
                IsLocked = false,
                RemainingAttempts = Math.Max(0, remainingAttempts),
                LockoutEndsAt = null,
                TotalAttempts = attempts
            };
        }

        /// <summary>
        /// Registra un intento de login fallido
        /// </summary>
        public void RecordFailedAttempt(string username, string ipAddress)
        {
            var key = GetKey(username, ipAddress);
            var now = DateTime.UtcNow;

            // Agregar el intento
            _failedAttempts.AddOrUpdate(
                key,
                _ => new List<DateTime> { now },
                (_, attempts) =>
                {
                    lock (attempts)
                    {
                        attempts.Add(now);
                        return attempts;
                    }
                });

            // Verificar si debe bloquearse
            var recentAttempts = GetRecentAttempts(key);
            if (recentAttempts >= MaxFailedAttempts)
            {
                _lockouts[key] = DateTime.UtcNow.Add(LockoutDuration);
            }
        }

        /// <summary>
        /// Resetea los intentos despues de un login exitoso
        /// </summary>
        public void ResetAttempts(string username, string ipAddress)
        {
            var key = GetKey(username, ipAddress);
            _failedAttempts.TryRemove(key, out _);
            _lockouts.TryRemove(key, out _);
        }

        /// <summary>
        /// Obtiene la cantidad de intentos recientes dentro de la ventana de tiempo
        /// </summary>
        private int GetRecentAttempts(string key)
        {
            if (!_failedAttempts.TryGetValue(key, out var attempts))
                return 0;

            var cutoff = DateTime.UtcNow.Subtract(AttemptWindow);
            lock (attempts)
            {
                return attempts.Count(a => a > cutoff);
            }
        }

        /// <summary>
        /// Limpia intentos antiguos para liberar memoria
        /// </summary>
        private void CleanupOldAttempts(string key)
        {
            if (!_failedAttempts.TryGetValue(key, out var attempts))
                return;

            var cutoff = DateTime.UtcNow.Subtract(AttemptWindow);
            lock (attempts)
            {
                attempts.RemoveAll(a => a <= cutoff);
            }

            // Si no quedan intentos, remover la entrada
            if (attempts.Count == 0)
            {
                _failedAttempts.TryRemove(key, out _);
            }
        }
    }
}
