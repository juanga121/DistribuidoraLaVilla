using System;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO para crear un movimiento de producto
    /// </summary>
    public class CrearMovimientoProductoDTO
    {
        public int IdLoteProducto { get; set; }
        public int TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public Guid? IdCliente { get; set; }
        public Guid? IdProveedor { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
    }
}
