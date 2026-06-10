namespace DistribuidoraLaVilla.Domain.DTOS.Caja
{
    public class CajaReporteDTO
    {
        public string Periodo { get; set; } = string.Empty;
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Neto { get; set; }
        public int CantidadMovimientos { get; set; }
        public List<CajaMovimientoDTO> Movimientos { get; set; } = new();
    }
}
