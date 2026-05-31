using System;

namespace DistribuidoraLaVilla.Domain.DTOS.Productos
{
    /// <summary>
    /// DTO para solicitar una orden de producción
    /// </summary>
    public class SolicitudProduccionDTO
    {
        public int IdProducto { get; set; }
        public decimal CantidadProducir { get; set; }
        public int IdUnidadMedida { get; set; }
        public Guid IdUsuario { get; set; }
        public string? Observaciones { get; set; }
    }
}
