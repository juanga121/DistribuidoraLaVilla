namespace DistribuidoraLaVilla.Domain.DTOS.Caja
{
    public class CajaAperturaDTO
    {
        public int IdApertura { get; set; }
        public Guid IdUsuario { get; set; }
        public string? UsuarioNombre { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinal { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal? Diferencia { get; set; }
        public int Estado { get; set; }
        public int CantidadMovimientos { get; set; }
        public Dictionary<string, decimal>? DesgloseMetodosPago { get; set; }
    }
}
