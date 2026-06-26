using Microsoft.EntityFrameworkCore;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
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
        public DbSet<PasivosEntity> Pasivos { get; set; }
        public DbSet<CategoriasProductosEntity> CategoriasProductos { get; set; }
        public DbSet<ProductosEntity> Productos { get; set; }
        public DbSet<LotesProductosEntity> LotesProductos { get; set; }
        public DbSet<MovimientosProductosEntity> MovimientosProductos { get; set; }
        public DbSet<RecetaProductoEntity> RecetasProducto { get; set; }
        public DbSet<OrdenProduccionEntity> OrdenesProduccion { get; set; }
        public DbSet<UsuariosEntity> Usuarios { get; set; }
        public DbSet<ClientesEntity> Clientes { get; set; }

        // ── Caja ──
        public DbSet<CajaAperturaEntity> CajaAperturas { get; set; }
        public DbSet<CajaMovimientoEntity> CajaMovimientos { get; set; }
        public DbSet<ReciboEntity> Recibos { get; set; }

        // ── Facturación ──
        public DbSet<FacturaEntity> Facturas { get; set; }
        public DbSet<DetalleFacturaEntity> DetallesFactura { get; set; }
        public DbSet<CuentasCobrarEntity> CuentasCobrar { get; set; }
        public DbSet<PagoCuentaEntity> PagosCuenta { get; set; }
        public DbSet<EstadoFacturaEntity> EstadosFactura { get; set; }
        public DbSet<TipoFacturaEntity> TiposFactura { get; set; }

        // ── Movimientos generales (productos terminados) ──
        public DbSet<MovimientoEntity> Movimientos { get; set; }

        // ── Auditoría ──
        public DbSet<AuditoriaEntity> Auditoria { get; set; }

        // ── Lookup compartidos ──
        public DbSet<EstadoEntity> Estados { get; set; }
    }
}
