using System;
using System.Collections.Generic;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO con el detalle de un ingrediente consumido
    /// </summary>
    public class ConsumoIngredienteDTO
    {
        public int IdMateriaPrima { get; set; }
        public string? NombreMateriaPrima { get; set; }
        public decimal CantidadRequerida { get; set; }
        public decimal CantidadConsumida { get; set; }
        public string? UnidadMedida { get; set; }
        public int IdMovimiento { get; set; }

        /// <summary>
        /// Costo real total del ingrediente consumido (Σ cantidad × costo unitario del lote de MP).
        /// </summary>
        public decimal CostoTotalConsumido { get; set; }
    }

    /// <summary>
    /// DTO de respuesta tras procesar una orden de producción
    /// </summary>
    public class ResultadoProduccionDTO
    {
        public int IdOrden { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public decimal CantidadProducida { get; set; }
        public string? UnidadMedida { get; set; }
        public int IdLoteGenerado { get; set; }
        public int IdMovimientoEntradaProducto { get; set; }
        public DateTime FechaProduccion { get; set; }

        /// <summary>
        /// Costo real total de la materia prima consumida para producir.
        /// </summary>
        public decimal CostoTotal { get; set; }
        public List<ConsumoIngredienteDTO> IngredientesConsumidos { get; set; } = new();
        public string? Observaciones { get; set; }
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
    }
}
