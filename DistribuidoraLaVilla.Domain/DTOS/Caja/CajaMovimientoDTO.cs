namespace DistribuidoraLaVilla.Domain.DTOS.Caja
{
    public class CajaMovimientoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdApertura { get; set; }
        public int TipoMovimiento { get; set; }
        public string TipoMovimientoDescripcion { get; set; } = string.Empty;
        public int? IdFactura { get; set; }
        public int? IdPago { get; set; }
        public int? IdRecibo { get; set; }
        public string? NumeroFactura { get; set; }
        public string? NumeroRecibo { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public Guid IdUsuario { get; set; }
        public string? UsuarioNombre { get; set; }
        public int? MetodoPago { get; set; }
        public string? MetodoPagoDescripcion { get; set; }
    }
}
