using System.ComponentModel.DataAnnotations;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO de entrada para la creación manual de movimientos de materia prima.
    /// </summary>
    public class MovimientoMateriaPrimaDTO
    {
        /// <summary>
        /// ID del lote de materia prima (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El lote de materia prima es requerido")]
        public int IdLoteMateria { get; set; }

        /// <summary>
        /// ID del tipo de movimiento (obligatorio):
        /// 1=Entrada, 2=Consumo, 3=Ajuste, 4=Devolución, 5=Vencimiento
        /// </summary>
        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public int IdTipoMovimiento { get; set; }

        /// <summary>
        /// Cantidad del movimiento. Debe ser mayor a 0.
        /// Para salidas (Consumo/Vencimiento) se valida stock suficiente.
        /// </summary>
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        /// <summary>
        /// ID de la unidad de medida (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "La unidad de medida es requerida")]
        public int IdUnidadMedida { get; set; }

        /// <summary>
        /// ID del usuario que realiza el movimiento (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El usuario es requerido")]
        public Guid IdUsuario { get; set; }

        /// <summary>
        /// Observación opcional del movimiento
        /// </summary>
        public string? Observacion { get; set; }
    }
}
