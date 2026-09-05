namespace DistribuidoraLaVilla.Domain.DTOS.CxC
{
    public class EstadoCuentaDTO
    {
        public string IdCliente { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteDocumento { get; set; } = string.Empty;
        public int TipoPersona { get; set; }
        public string TipoPersonaDescripcion { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public decimal CreditoDisponible { get; set; }
        public List<CuentaCobrarDTO> FacturasPendientes { get; set; } = new();
        public List<PagoDTO> PagosRecientes { get; set; } = new();
        public decimal SaldoTotalPendiente { get; set; }
    }
}
