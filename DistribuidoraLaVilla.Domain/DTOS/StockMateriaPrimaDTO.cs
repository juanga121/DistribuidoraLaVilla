using System;
using System.Collections.Generic;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO para información detallada de un lote en el stock
    /// </summary>
    public class LoteStockDTO
    {
        public int IdLote { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaEntrada { get; set; }
        public string? NombreMarca { get; set; }
        public string? NombreProveedor { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotalLote { get; set; }
        public int? DiasParaVencimiento { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para consulta de stock de materia prima
    /// </summary>
    public class StockMateriaPrimaDTO
    {
        public int IdMateriaPrima { get; set; }
        public string? NombreMateriaPrima { get; set; }
        public decimal StockDisponible { get; set; }
        public int IdUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public string? SimboloUnidadMedida { get; set; }
        public int LotesDisponibles { get; set; }
        public DateTime? ProximaFechaVencimiento { get; set; }
        public decimal CostoPromedio { get; set; }
        public decimal ValorTotalStock { get; set; }
        public List<LoteStockDTO> DetalleLotes { get; set; } = new();
    }
}
