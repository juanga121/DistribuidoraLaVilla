namespace DistribuidoraLaVilla.Domain.DTOS.CxC
{
    public class PagoDTO
    {
        public int Id { get; set; }
        public int IdCxc { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoPago { get; set; }
        public int MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public string? Observacion { get; set; }
    }
}
