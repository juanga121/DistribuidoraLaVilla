namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO de respuesta para movimientos de materia prima.
    /// Usado en creación manual (POST) y futuros endpoints individuales.
    /// Incluye TipoMovimientoNombre resuelto y StockAnterior/StockNuevo
    /// para trazabilidad completa.
    /// </summary>
    public class MovimientoMateriaPrimaResponseDTO
    {
        /// <summary>ID del movimiento</summary>
        public int Id { get; set; }

        /// <summary>ID del lote de materia prima asociado</summary>
        public int IdLoteMateria { get; set; }

        /// <summary>ID del tipo de movimiento</summary>
        public int IdTipoMovimiento { get; set; }

        /// <summary>Nombre del tipo de movimiento (resuelto)</summary>
        public string? TipoMovimientoNombre { get; set; }

        /// <summary>Fecha y hora del movimiento</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Cantidad del movimiento (positiva o negativa según tipo)</summary>
        public decimal Cantidad { get; set; }

        /// <summary>ID de la unidad de medida</summary>
        public int IdUnidadMedida { get; set; }

        /// <summary>ID del usuario que realizó el movimiento</summary>
        public Guid IdUsuario { get; set; }

        /// <summary>Observación del movimiento</summary>
        public string? Observacion { get; set; }

        /// <summary>Stock disponible del lote INMEDIATAMENTE ANTES del movimiento</summary>
        public decimal StockAnterior { get; set; }

        /// <summary>Stock disponible del lote INMEDIATAMENTE DESPUÉS del movimiento</summary>
        public decimal StockNuevo { get; set; }
    }
}
