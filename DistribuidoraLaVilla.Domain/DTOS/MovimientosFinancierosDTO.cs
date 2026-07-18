namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class MovimientosFinancierosDTO
    {
        public int IdMovimiento { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public string? SubTipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string? OrigenModulo { get; set; }
        public int? ReferenciaId { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public int Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? IdUsuario { get; set; }
    }

    public class CrearMovimientoFinancieroDTO
    {
        public string TipoMovimiento { get; set; } = string.Empty;
        public string? SubTipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Direccion { get; set; } = "Neutro";
        public string? OrigenModulo { get; set; }
        public int? ReferenciaId { get; set; }
        public DateTime? FechaMovimiento { get; set; }
        public int? Estado { get; set; }
    }
}
