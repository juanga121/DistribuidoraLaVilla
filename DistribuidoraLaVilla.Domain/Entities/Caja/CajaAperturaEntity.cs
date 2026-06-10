using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLaVilla.Domain.Entities.Caja
{
    [Table("caja_apertura")]
    public class CajaAperturaEntity
    {
        [Key]
        [Column("id_apertura")]
        public int Id { get; set; }

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("fecha_apertura")]
        public DateTime FechaApertura { get; set; }

        [Column("monto_inicial")]
        public decimal MontoInicial { get; set; }

        [Column("fecha_cierre")]
        public DateTime? FechaCierre { get; set; }

        [Column("monto_final")]
        public decimal? MontoFinal { get; set; }

        [Column("total_ingresos")]
        public decimal? TotalIngresos { get; set; }

        [Column("total_egresos")]
        public decimal? TotalEgresos { get; set; }

        [Column("diferencia")]
        public decimal? Diferencia { get; set; }

        [Column("estado")]
        public int Estado { get; set; }
    }
}
