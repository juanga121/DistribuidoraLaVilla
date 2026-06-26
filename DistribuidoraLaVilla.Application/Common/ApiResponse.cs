namespace DistribuidoraLaVilla.Application.Common
{
    /// <summary>
    /// Respuesta genérica estándar del API.
    /// Todos los endpoints deben usar este envoltorio para consistencia
    /// con el frontend (success, message, data).
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>Indica si la operación fue exitosa</summary>
        public bool Success { get; set; } = true;

        /// <summary>Mensaje descriptivo del resultado</summary>
        public string? Message { get; set; }

        /// <summary>Datos de respuesta</summary>
        public T? Data { get; set; }

        /// <summary>
        /// Crea una respuesta exitosa con datos
        /// </summary>
        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        /// <summary>
        /// Crea una respuesta de error
        /// </summary>
        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
