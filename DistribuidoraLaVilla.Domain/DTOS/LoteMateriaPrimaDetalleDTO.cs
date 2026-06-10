using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    /// <summary>
    /// DTO de respuesta para lotes de materia prima con nombres resueltos
    /// </summary>
    public class LoteMateriaPrimaDetalleDTO
    {
        public int Id { get; set; }
        public int IdMarca { get; set; }
        public string? NombreMarca { get; set; }
        public int IdMateria { get; set; }
        public string? NombreMateria { get; set; }
        public Guid IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public string? SimboloUnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public int Estado { get; set; }
        public string? NombreEstado { get; set; }
        public int? DiasParaVencimiento { get; set; }
    }
}
