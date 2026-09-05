using System;
using System.Collections.Generic;

namespace DistribuidoraLaVilla.Domain.DTOS.Compras
{
    /// <summary>
    /// DTO de impresión para el comprobante de recepción de compra (CA06).
    /// Contiene los datos ya generados por el flujo de recepción para
    /// imprimir en formato térmico (80mm) o convencional (A4) sin volver
    /// a solicitar información al usuario.
    /// </summary>
    public class ComprobanteCompraDTO
    {
        public int IdRecepcionCompra { get; set; }

        public int IdOrdenCompra { get; set; }

        public string? NumeroFacturaProveedor { get; set; }

        public DateTime FechaFactura { get; set; }

        public DateTime FechaRecepcion { get; set; }

        public string? ProveedorNombre { get; set; }

        public int FormaPago { get; set; }

        public string? FormaPagoTexto { get; set; }

        public string? UsuarioNombre { get; set; }

        public List<ComprobanteCompraDetalleDTO> Detalles { get; set; } = new();

        public decimal Total { get; set; }
    }

    /// <summary>
    /// Una línea del comprobante de recepción de compra.
    /// </summary>
    public class ComprobanteCompraDetalleDTO
    {
        public string? ProductoNombre { get; set; }

        public decimal Cantidad { get; set; }

        public string? UnidadMedida { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}