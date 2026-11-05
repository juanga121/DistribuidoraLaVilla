namespace DistribuidoraLaVilla.Domain.DTOS.Reportes
{
    public class InventarioReporteDTO
    {
        public List<InventarioItemDTO> Productos { get; set; } = new();
        public List<LoteProximoVencerDTO> LotesProximosVencer { get; set; } = new();
    }

    public class InventarioItemDTO
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public bool StockBajo { get; set; }

        /// <summary>Minimum stock before the product is reported as low, in the mode unit.</summary>
        public decimal StockMinimo { get; set; }

        /// <summary>Inventory mode of the product: 1 = Unidad, 2 = Peso, 3 = Peso variable.</summary>
        public int ModoInventario { get; set; } = 1;

        /// <summary>Unit of <see cref="StockActual"/>: "kg" in weight modes, "und." in unit mode.</summary>
        public string SimboloUnidad { get; set; } = "und.";
    }

    public class LoteProximoVencerDTO
    {
        public int IdLote { get; set; }
        public string Producto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }

        /// <summary>Inventory mode of the product: 1 = Unidad, 2 = Peso, 3 = Peso variable.</summary>
        public int ModoInventario { get; set; } = 1;

        /// <summary>Unit of <see cref="Cantidad"/>: "kg" in weight modes, "und." in unit mode.</summary>
        public string SimboloUnidad { get; set; } = "und.";
    }
}
