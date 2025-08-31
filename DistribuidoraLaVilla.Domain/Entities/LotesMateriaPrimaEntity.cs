using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    [Table("lotes_materia_prima")]
    public class LotesMateriaPrimaEntity
    {
        [Key]
        [Column("id_lote_materia")]
        public int Id { get; set; }

        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Column("id_materia")]
        public int IdMateria { get; set; }

        [Column("id_proveedor")]
        public Guid IdProveedor { get; set; }

        [Column("fecha_entrada")]
        public DateTime FechaEntrada { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("costo_unitario")]
        public decimal CostoUnitario { get; set; }

        [Column("costo_total")]
        public decimal CostoTotal { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
