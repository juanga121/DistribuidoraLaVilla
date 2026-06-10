using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO de respuesta para listado de movimientos con nombres resueltos
    /// </summary>
    public class MovimientoMateriaPrimaListDTO
    {
        public int Id { get; set; }
        public int IdLoteMateria { get; set; }
        public string? NombreLote { get; set; }
        public int IdTipoMovimiento { get; set; }
        public string? TipoMovimientoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public string? SimboloUnidadMedida { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
    }
}
