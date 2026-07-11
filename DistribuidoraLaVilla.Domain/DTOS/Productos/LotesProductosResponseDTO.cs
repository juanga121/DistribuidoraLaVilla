using System;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO de respuesta para lotes de productos con nombres resueltos
    /// </summary>
    public class LotesProductosResponseDTO
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public Guid IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CantidadUnidades { get; set; }
        public decimal PesoTotal { get; set; }
        public int IdUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public string? SimboloUnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioKilo { get; set; }
        public decimal PrecioTotal { get; set; }
        public int IdMarca { get; set; }
        public string? NombreMarca { get; set; }
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal PesoDisponible { get; set; }
        public int Estado { get; set; }
    }
}
