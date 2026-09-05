using Microsoft.EntityFrameworkCore;
using DistribuidoraLaVilla.Domain.Entities;
using DistribuidoraLaVilla.Domain.Entities.Caja;
using DistribuidoraLaVilla.Domain.Entities.Facturacion;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Entities.Compras;
using DistribuidoraLaVilla.Domain.Enums;

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
        public DbSet<ActivosEntity> Activos { get; set; }
        public DbSet<PatrimonioEntity> Patrimonio { get; set; }
        public DbSet<CategoriasProductosEntity> CategoriasProductos { get; set; }
        public DbSet<ProductosEntity> Productos { get; set; }
        public DbSet<LotesProductosEntity> LotesProductos { get; set; }
        public DbSet<MovimientosProductosEntity> MovimientosProductos { get; set; }
        public DbSet<RecetaProductoEntity> RecetasProducto { get; set; }
        public DbSet<OrdenProduccionEntity> OrdenesProduccion { get; set; }
        public DbSet<UsuariosEntity> Usuarios { get; set; }
        public DbSet<ClientesEntity> Clientes { get; set; }

        public DbSet<CajaAperturaEntity> CajaAperturas { get; set; }
        public DbSet<CajaMovimientoEntity> CajaMovimientos { get; set; }
        public DbSet<ReciboEntity> Recibos { get; set; }

        public DbSet<FacturaEntity> Facturas { get; set; }
        public DbSet<DetalleFacturaEntity> DetallesFactura { get; set; }
        public DbSet<CuentasCobrarEntity> CuentasCobrar { get; set; }
        public DbSet<PagoCuentaEntity> PagosCuenta { get; set; }
        public DbSet<EstadoFacturaEntity> EstadosFactura { get; set; }
        public DbSet<TipoFacturaEntity> TiposFactura { get; set; }

        public DbSet<OrdenCompraEntity> OrdenesCompra { get; set; }
        public DbSet<DetalleCompraEntity> DetallesCompra { get; set; }
        public DbSet<RecepcionCompraEntity> RecepcionesCompra { get; set; }

        public DbSet<MovimientoEntity> Movimientos { get; set; }

        public DbSet<AuditoriaEntity> Auditoria { get; set; }

        public DbSet<CuentasPagarEntity> CuentasPagar { get; set; }

        public DbSet<PagoCxPEntity> PagosCxp { get; set; }

        public DbSet<MovimientosFinancierosEntity> MovimientosFinancieros { get; set; }

        public DbSet<EstadoEntity> Estados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

modelBuilder.Entity<CuentasPagarEntity>(entity =>
            {
                entity.HasIndex(e => new { e.Estado, e.SaldoPendiente })
                    .HasDatabaseName("IX_cuentas_pagar_estado_saldo");

                entity.HasIndex(e => e.IdProveedor)
                    .HasDatabaseName("IX_cuentas_pagar_proveedor");
            });

            modelBuilder.Entity<PagoCxPEntity>(entity =>
            {
                entity.HasIndex(e => e.IdCuentaPagar)
                    .HasDatabaseName("IX_pagos_cxp_cuenta_pagar");

                entity.HasOne<CuentasPagarEntity>()
                    .WithMany()
                    .HasForeignKey(e => e.IdCuentaPagar)
                    .HasConstraintName("FK_pagos_cxp_cuentas_pagar");
            });

modelBuilder.Entity<RecepcionCompraEntity>(entity =>
            {
                entity.HasIndex(e => e.IdOrdenCompra)
                    .IsUnique()
                    .HasDatabaseName("IX_recepciones_compra_orden");

                entity.Property(e => e.FormaPago)
                    .HasDefaultValue((int)FormaPago.Credito);
            });

            modelBuilder.Entity<ClientesEntity>(entity =>
            {
                entity.Property(e => e.TipoPersona)
                    .HasDefaultValue((int)TipoPersona.Natural);
            });

            modelBuilder.Entity<LotesProductosEntity>(entity =>
            {
                entity.HasOne<RecepcionCompraEntity>()
                    .WithMany()
                    .HasForeignKey(e => e.IdRecepcionCompra)
                    .HasConstraintName("FK_lotes_productos_recepciones_compra");
            });
        }
    }
}
