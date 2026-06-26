namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO para tipos de movimiento de materia prima.
    /// Usado en el endpoint ObtenerTiposMovimiento con ApiResponse.
    /// </summary>
    public class TipoMovimientoDTO
    {
        /// <summary>ID del tipo de movimiento</summary>
        public int Id { get; set; }

        /// <summary>Nombre del tipo de movimiento (Entrada, Consumo, Ajuste, etc.)</summary>
        public string? Nombre { get; set; }

        /// <summary>Descripción del tipo de movimiento</summary>
        public string? Descripcion { get; set; }
    }
}
