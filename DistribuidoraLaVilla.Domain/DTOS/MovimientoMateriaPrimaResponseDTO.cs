using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class MovimientoMateriaPrimaResponseDTO
    {
        public int Id { get; set; }
        public int IdLoteMateria { get; set; }
        public int IdTipoMovimiento { get; set; }
        public string? TipoMovimientoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
    }
}
