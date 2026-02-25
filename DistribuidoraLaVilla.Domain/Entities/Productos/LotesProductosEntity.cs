using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities.Productos
{
    [Table("lotes_productos")]
    public class LotesProductosEntity
    {
        [Key]
        [Column("id_lote")]
        public int Id { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("id_proveedor")]
        public Guid IdProveedor { get; set; }

        [Column("fecha_entrada")]
        public DateTime FechaEntrada { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [Column("cantidad_unidades")]
        public int CantidadUnidades { get; set; }

        [Column("peso_total")]
        public decimal PesoTotal { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("precio_kilo")]
        public decimal PrecioKilo { get; set; }

        [Column("precio_total")]
        public decimal PrecioTotal { get; set; }

        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
