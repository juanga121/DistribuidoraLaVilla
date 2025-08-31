using DistribuidoraLaVilla.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.Entities
{
    public class MovimientosMateriaPrimaEntity
    {
        [Key]
        [Column("id_movimiento")]
        public int Id { get; set; }

        [Column("id_lote_materia")]
        public int IdLoteMateria { get; set; }

        [Column("tipo_movimiento")]
        public int IdTipoMovimiento { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("id_unidad_medida")]
        public int IdUnidadMedida { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("observacion")]
        public string? Observacion { get; set; }

    }
}
