namespace DistribuidoraLaVilla.Domain.DTOS.CxC
{
    public class PagoResponseDTO
    {
        public bool Exitoso { get; set; }
        public int? IdPago { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
