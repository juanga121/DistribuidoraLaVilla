namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    /// <summary>
    /// Rentabilidad de las ventas en un período: ingreso, costo real de la mercadería vendida
    /// y margen resultante, por producto.
    /// </summary>
    public class RentabilidadReporteDTO
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal TotalIngreso { get; set; }
        public decimal TotalCosto { get; set; }
        public decimal MargenTotal { get; set; }
        public decimal MargenPorcentaje { get; set; }

        /// <summary>Cantidad de productos que se vendieron por debajo de su costo.</summary>
        public int ProductosBajoCosto { get; set; }

        public List<RentabilidadProductoDTO> Productos { get; set; } = new();
    }

    public class RentabilidadProductoDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int ModoInventario { get; set; }
        public string SimboloUnidad { get; set; } = "und.";
        public decimal UnidadesVendidas { get; set; }
        public decimal KgVendidos { get; set; }
        public decimal Ingreso { get; set; }
        public decimal Costo { get; set; }
        public decimal Margen { get; set; }
        public decimal MargenPorcentaje { get; set; }

        /// <summary>True si el margen es negativo: se vendió por debajo del costo.</summary>
        public bool VendeBajoCosto { get; set; }
    }
}
