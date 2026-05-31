using System;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO de respuesta tras registrar un movimiento de producto
    /// </summary>
    public class RespuestaMovimientoProductoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdLoteProducto { get; set; }
        public int TipoMovimiento { get; set; }
        public string? TipoMovimientoNombre { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public decimal TotalMovimiento { get; set; }
        public int IdUnidadMedida { get; set; }
        public Guid? IdCliente { get; set; }
        public Guid? IdProveedor { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
        public int Estado { get; set; }
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
    }
}
