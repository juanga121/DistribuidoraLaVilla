namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class MovimientosDiariosDTO
    {
        public string Fecha { get; set; } = string.Empty;

        public ResumenFacturasDTO Facturas { get; set; } = new();

        public ResumenMovimientosMPDTO MateriaPrima { get; set; } = new();

        public ResumenMovimientosProductosDTO Productos { get; set; } = new();

        public ResumenCxcDiarioDTO CuentasCobrar { get; set; } = new();

        public List<string> Alertas { get; set; } = new();
    }

    public class ResumenFacturasDTO
    {
        public int Cantidad { get; set; }
        public int Contado { get; set; }
        public int Credito { get; set; }
        public decimal Total { get; set; }
    }

    public class ResumenMovimientosMPDTO
    {
        public int Entradas { get; set; }
        public int Consumos { get; set; }
        public int Ajustes { get; set; }
        public int Devoluciones { get; set; }
        public int Vencimientos { get; set; }
    }

    public class ResumenMovimientosProductosDTO
    {
        public int Entradas { get; set; }
        public int Ventas { get; set; }
        public int Ajustes { get; set; }
        public int Devoluciones { get; set; }
        public int Vencimientos { get; set; }
    }

    public class ResumenCxcDiarioDTO
    {
        public int Pendientes { get; set; }
        public int Vencidas { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalVencido { get; set; }
    }
}
