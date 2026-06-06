using System;
using System.Collections.Generic;

namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    /// <summary>
    /// DTO para impresión POS/ticket de factura.
    /// Contiene los datos formateados para enviar a una impresora térmica.
    /// La implementación de la impresión (Epson, XPrinter, SAT) se hará
    /// en una fase posterior; este DTO prepara la capa de datos.
    /// </summary>
    public class FacturaTicketDTO
    {
        // ── Cabecera ──
        public string? EmpresaNombre { get; set; }
        public string? EmpresaDireccion { get; set; }
        public string? EmpresaTelefono { get; set; }
        public string? EmpresaCuit { get; set; }
        public string? EmpresaEmail { get; set; }

        // ── Factura ──
        public int IdFactura { get; set; }
        public string? NumeroFactura { get; set; }
        public string? Fecha { get; set; }
        public string? TipoFactura { get; set; }      // Contado / Crédito
        public string? FormaPago { get; set; }         // Efectivo / Transferencia / etc
        public string? MetodoPago { get; set; }

        // ── Cliente ──
        public string? ClienteNombre { get; set; }
        public string? ClienteDocumento { get; set; }
        public string? ClienteDireccion { get; set; }
        public string? ClienteTelefono { get; set; }

        // ── Líneas ──
        public List<LineaTicketDTO> Lineas { get; set; } = new();

        // ── Totales ──
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string? TotalEnLetras { get; set; }

        // ── Pie ──
        public string? CajeroNombre { get; set; }
        public string? MensajePie { get; set; }
    }

    /// <summary>
    /// Una línea de detalle en el ticket
    /// </summary>
    public class LineaTicketDTO
    {
        public string? ProductoNombre { get; set; }
        public decimal Cantidad { get; set; }
        public string? UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? PesoTotal { get; set; }
        public decimal? PrecioKilo { get; set; }
        public decimal Subtotal { get; set; }
    }
}
