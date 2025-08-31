using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistribuidoraLaVilla.Domain.Entities;

namespace DistribuidoraLaVilla.Infrastructure.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<MarcaEntity> Marcas { get; set; }
        public DbSet<UnidadMedidaEntity> UnidadMedida { get; set; }
        public DbSet<TipoMovimientoMateriaPrimaEntity> TiposMovimientoMateriaPrima { get; set; }
        public DbSet<MovimientosMateriaPrimaEntity> MovimientosMateriaPrima { get; set; }
        public DbSet<LotesMateriaPrimaEntity> LotesMateriaPrima { get; set; }
        public DbSet<MateriaPrimaEntity> MateriaPrima { get; set; }
        public DbSet<CategoriaMateriaPrimaEntity> CategoriasMateriaPrima { get; set; }
    }
}
