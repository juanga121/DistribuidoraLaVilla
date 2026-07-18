namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class BalanceMinimoReporteDTO
    {
        public DateTime FechaGeneracion { get; set; }
        public decimal CuentasPorCobrarTotal { get; set; }
        public decimal InventarioProductosTotal { get; set; }
        public decimal InventarioMateriaPrimaTotal { get; set; }
        public decimal EfectivoEquivalenteTotal { get; set; }
        public decimal ActivosTotal { get; set; }
        public decimal PasivosTotal { get; set; }
        public decimal CuentasPagarTotal { get; set; }
        public decimal PatrimonioComputed { get; set; }
        public decimal PatrimonioTotal { get; set; }
        public decimal PatrimonioCapitalTotal { get; set; }
        public string ActivosNota { get; set; } = string.Empty;
        public string PasivosNota { get; set; } = string.Empty;
        public string PatrimonioNota { get; set; } = string.Empty;
        public List<BalanceCxcItemDTO> CuentasPorCobrar { get; set; } = new();
        public List<BalanceProductoItemDTO> ProductosTerminados { get; set; } = new();
        public List<BalanceMateriaPrimaItemDTO> MateriaPrima { get; set; } = new();
        public List<BalanceActivoItemDTO> Activos { get; set; } = new();
        public List<BalanceEfectivoItemDTO> EfectivoItems { get; set; } = new();
        public List<BalanceCuentasPagarItemDTO> CuentasPagarItems { get; set; } = new();
        public List<BalancePatrimonioItemDTO> Patrimonios { get; set; } = new();
        public List<BalancePatrimonioItemDTO> PatrimonioCapitalItems { get; set; } = new();
    }

    public class BalanceActivoItemDTO
    {
        public int IdActivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
    }

    public class BalancePatrimonioItemDTO
    {
        public int IdPatrimonio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
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

    public class BalanceEfectivoItemDTO
    {
        public int IdApertura { get; set; }
        public DateTime FechaApertura { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal Saldo { get; set; }
    }

    public class BalanceCuentasPagarItemDTO
    {
        public int IdCuentaPagar { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}
