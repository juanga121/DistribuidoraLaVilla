namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class CrearCxPDTO
    {
        public Guid IdProveedor { get; set; }
        public int? IdOrdenCompra { get; set; }
        public decimal MontoTotal { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public Guid? IdUsuario { get; set; }
    }

    public class CuentasPagarDTO
    {
        public int IdCuentaPagar { get; set; }
        public Guid IdProveedor { get; set; }
        public string? ProveedorNombre { get; set; }
        public int? IdOrdenCompra { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal MontoPagado { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int Estado { get; set; }
        public string? EstadoDescripcion { get; set; }
        public int DiasVencidos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public Guid? IdUsuario { get; set; }
    }

    public class RegistrarPagoCxPDTO
    {
        public decimal Monto { get; set; }
        public Guid? IdUsuario { get; set; }
        public int? MetodoPago { get; set; }
        public string? Observacion { get; set; }
    }

    public class PagoCxPDTO
    {
        public int IdPago { get; set; }
        public int IdCuentaPagar { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public Guid? IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? MetodoPago { get; set; }
        public string? MetodoPagoDescripcion { get; set; }
        public string? Observacion { get; set; }
    }
}
