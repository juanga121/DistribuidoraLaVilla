using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class LotesMateriaPrimaDTO
    {
        public int IdMarca { get; set; }
        public int IdMateria { get; set; }
        public Guid IdProveedor { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
