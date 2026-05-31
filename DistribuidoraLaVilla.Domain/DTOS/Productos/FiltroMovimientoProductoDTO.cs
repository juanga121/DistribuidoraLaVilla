using System;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO para filtrar movimientos de productos
    /// </summary>
    public class FiltroMovimientoProductoDTO
    {
        public int? IdLoteProducto { get; set; }
        public int? TipoMovimiento { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public Guid? IdCliente { get; set; }
        public Guid? IdProveedor { get; set; }
        public Guid? IdUsuario { get; set; }
        public int? Estado { get; set; }
    }
}
