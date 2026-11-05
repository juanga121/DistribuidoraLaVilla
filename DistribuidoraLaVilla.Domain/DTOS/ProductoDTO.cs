using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class ProductoDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int IdCategoria { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool VentaPorPeso { get; set; }
        public decimal? PrecioPorKilo { get; set; }
        public decimal? PesoPorUnidad { get; set; }
        public int ModoInventario { get; set; }

        /// <summary>
        /// Minimum stock before the product is reported as low, in the product's mode unit.
        /// Null or omitted means the mode default applies.
        /// </summary>
        public decimal? StockMinimo { get; set; }
    }
}
