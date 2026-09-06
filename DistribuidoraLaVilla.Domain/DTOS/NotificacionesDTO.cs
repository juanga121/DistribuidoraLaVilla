using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// Resumen consolidado de alertas operativas para la pantalla de notificaciones.
    /// </summary>
    public class NotificacionesResumenDTO
    {
        public DateTime FechaConsulta { get; set; }
        public int DiasAnticipacion { get; set; }
        public int TotalAlertas { get; set; }
        public System.Collections.Generic.List<NotificacionCxcDTO> CuentasCobrar { get; set; } = new();
        public System.Collections.Generic.List<NotificacionCxpDTO> CuentasPagar { get; set; } = new();
        public System.Collections.Generic.List<NotificacionStockDTO> StockProductos { get; set; } = new();
        public System.Collections.Generic.List<NotificacionStockDTO> StockMateriaPrima { get; set; } = new();
    }

    /// <summary>
    /// Alerta de cuenta por cobrar (cliente debe).
    /// </summary>
    public class NotificacionCxcDTO
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal MontoPagado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int DiasVencidos { get; set; }
    }

    /// <summary>
    /// Alerta de cuenta por pagar (deuda con proveedor).
    /// </summary>
    public class NotificacionCxpDTO
    {
        public int IdCuentaPagar { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int Estado { get; set; }
        public string EstadoDescripcion { get; set; } = string.Empty;
        public int DiasVencidos { get; set; }
    }

    /// <summary>
    /// Alerta de stock (producto o materia prima) con lote próximo a vencer.
    /// </summary>
    public class NotificacionStockDTO
    {
        public int IdLote { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public int UnidadMedidaId { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasParaVencer { get; set; }
    }
}