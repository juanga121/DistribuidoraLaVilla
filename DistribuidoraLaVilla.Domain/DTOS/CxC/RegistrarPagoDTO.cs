namespace DistribuidoraLaVilla.Domain.DTOS.CxC
{
    public class RegistrarPagoDTO
    {
        public int IdCxc { get; set; }
        public decimal MontoPago { get; set; }
        public int MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
    }
}
