using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Productos;

namespace DistribuidoraLaVilla.Infrastructure.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<MarcasEntity> Marcas { get; set; }
        public DbSet<UnidadMedidaEntity> UnidadMedida { get; set; }
        public DbSet<TipoMovimientoMateriaPrimaEntity> TiposMovimientoMateriaPrima { get; set; }
        public DbSet<MovimientosMateriaPrimaEntity> MovimientosMateriaPrima { get; set; }
        public DbSet<LotesMateriaPrimaEntity> LotesMateriaPrima { get; set; }
        public DbSet<MateriaPrimaEntity> MateriaPrima { get; set; }
        public DbSet<CategoriaMateriaPrimaEntity> CategoriasMateriaPrima { get; set; }
        public DbSet<ProveedoresEntity> Proveedores { get; set; }
        public DbSet<CategoriasProductosEntity> CategoriasProductos { get; set; }
        public DbSet<ProductosEntity> Productos { get; set; }
        public DbSet<LotesProductosEntity> LotesProductos { get; set; }
    }
}
