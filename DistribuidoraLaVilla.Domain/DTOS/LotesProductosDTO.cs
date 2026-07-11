using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class LotesProductosDTO
    {
        public int IdProducto { get; set; }
        public Guid IdProveedor { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CantidadUnidades { get; set; }
        public decimal PesoTotal { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioKilo { get; set; }
        public int IdMarca { get; set; }
        public Guid IdUsuario { get; set; }
        public decimal PesoDisponible { get; set; }
    }
}
