namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class ClienteReporteDTO
    {
        public Guid IdCliente { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteDocumento { get; set; } = string.Empty;
        public List<FacturaClienteDTO> Facturas { get; set; } = new();
        public decimal TotalGastado { get; set; }
        public int CantidadFacturas { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public decimal CxcSaldoPendiente { get; set; }
    }

    public class FacturaClienteDTO
    {
        public int IdFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
