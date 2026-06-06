namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class MovimientosReporteDTO
    {
        public List<MovimientoItemDTO> Movimientos { get; set; } = new();
    }

    public class MovimientoItemDTO
    {
        public DateTime Fecha { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
