namespace DistribuidoraLaVilla.Domain.DTOS.Caja
{
    public class RegistrarEgresoDTO
    {
        public Guid IdUsuario { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public int? MetodoPago { get; set; }
    }
}
