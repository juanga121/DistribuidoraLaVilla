using System;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class MovimientoMateriaPrimaDTO
    {
        public int IdLoteMateria { get; set; }
        public int IdTipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observacion { get; set; }
    }
}
