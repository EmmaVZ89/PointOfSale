using System.Collections.Generic;

namespace PuntoDeVenta.API.DTOs
{
    /// <summary>
    /// Respuesta estandar de la API.
    ///
    /// TECNOLOGIA NUEVA: DTO (Data Transfer Object)
    /// - Los DTOs son objetos que transfieren datos entre capas
    /// - Exponen solo los campos necesarios (seguridad)
    /// - Desacoplan la API de las entidades de BD
    /// - Permiten versionar la API sin afectar el modelo interno
    /// </summary>
    /// <typeparam name="T">Tipo de datos a retornar</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica si la operacion fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Datos retornados (solo si Success = true)
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Lista de errores (solo si Success = false)
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Crea una respuesta exitosa
        /// </summary>
        public static ApiResponse<T> Ok(T data, string message = "Operacion exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        /// <summary>
        /// Crea una respuesta de error
        /// </summary>
        public static ApiResponse<T> Error(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Respuesta paginada para listados grandes
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
