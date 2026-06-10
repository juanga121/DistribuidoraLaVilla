namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class BalanceMinimoReporteDTO
    {
        public DateTime FechaGeneracion { get; set; }
        public decimal CuentasPorCobrarTotal { get; set; }
        public decimal InventarioProductosTotal { get; set; }
        public decimal InventarioMateriaPrimaTotal { get; set; }
        public decimal ActivosTotal { get; set; }
        public decimal PasivosTotal { get; set; }
        public decimal PatrimonioTotal { get; set; }
        public string PasivosNota { get; set; } = string.Empty;
        public List<BalanceCxcItemDTO> CuentasPorCobrar { get; set; } = new();
        public List<BalanceProductoItemDTO> ProductosTerminados { get; set; } = new();
        public List<BalanceMateriaPrimaItemDTO> MateriaPrima { get; set; } = new();
    }

    public class BalanceCxcItemDTO
    {
        public int IdFactura { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal SaldoPendiente { get; set; }
    }

    public class BalanceProductoItemDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class BalanceMateriaPrimaItemDTO
    {
        public int IdMateriaPrima { get; set; }
        public string NombreMateriaPrima { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
