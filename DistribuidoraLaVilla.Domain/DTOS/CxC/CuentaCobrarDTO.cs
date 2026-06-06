namespace DistribuidoraLaVilla.Domain.DTOS.CxC
{
    public class CuentaCobrarDTO
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public string IdCliente { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int DiasVencidos { get; set; }
    }
}
