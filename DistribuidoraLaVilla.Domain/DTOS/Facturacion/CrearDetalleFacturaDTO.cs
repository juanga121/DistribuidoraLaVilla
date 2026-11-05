namespace DistribuidoraLaVilla.Domain.DTOS.Facturacion
{
    public class CrearDetalleFacturaDTO
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal Precio { get; set; }
        public bool? EsVentaPorPeso { get; set; }

        /// <summary>
        /// Peso real de la línea, en kilogramos. Obligatorio cuando un producto mixto se vende por
        /// unidad: es lo que mueve el stock, sin conversiones.
        /// </summary>
        public decimal? PesoTotal { get; set; }
    }
}
