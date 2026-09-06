using DistribuidoraLaVilla.Application.Interfaces;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Application.Services.Auditoria;
using DistribuidoraLaVilla.Application.Services.Caja;
using DistribuidoraLaVilla.Application.Services.Compras;
using DistribuidoraLaVilla.Application.Services.CxC;
using DistribuidoraLaVilla.Application.Services.Facturacion;
using DistribuidoraLaVilla.Application.Services.Inventario;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Application.Services.Reportes;
using DistribuidoraLaVilla.Domain.Entities.Productos;
using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Infrastructure.Data;
using DistribuidoraLaVilla.Infrastructure.Repositories;

namespace DistribuidoraLaVilla.Api.Configurations
{
    public static class Injections
    {
        public static IServiceCollection AddRepositoryDependency(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<CategoriasMateriaPrimaService>();
            services.AddScoped<MateriaPrimaService>();
            services.AddScoped<ProveedoresService>();
            services.AddScoped<PasivosService>();
            services.AddScoped<ActivosService>();
            services.AddScoped<PatrimonioService>();
            services.AddScoped<MarcasService>();
            services.AddScoped<LotesMateriaPrimaService>();
            services.AddScoped<MovimientosMateriaPrimaService>();
            services.AddScoped<StockMateriaPrimaService>();
            services.AddScoped<UnidadMedidaService>();
            services.AddScoped<CategoriasProductosService>();
            services.AddScoped<ProductosService>();
            services.AddScoped<LotesProductosService>();
            services.AddScoped<MovimientosProductosService>();
            services.AddScoped<RecetaProductoService>();
            services.AddScoped<ProduccionService>();
            services.AddScoped<UsuariosService>();
            services.AddScoped<ClientesService>();
            services.AddScoped<FacturaService>();
            services.AddScoped<OrdenCompraService>();
            services.AddScoped<ICuentasCobrarService, CuentasCobrarService>();
            services.AddScoped<ICuentasPagarService, CuentasPagarService>();
            services.AddScoped<IInventarioService, InventarioService>();
            services.AddScoped<IReportesService, ReportesService>();
            services.AddScoped<IAuditoriaService, AuditoriaService>();
            services.AddScoped<MovimientosFinancierosService>();
            services.AddScoped<NotificacionesService>();
            services.AddScoped<ICajaService, CajaService>();
            services.AddScoped<IRecibosCajaService, RecibosCajaService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
