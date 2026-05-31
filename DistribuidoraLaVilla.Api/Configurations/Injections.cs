using DistribuidoraLaVilla.Domain.Interfaces;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
using DistribuidoraLaVilla.Application.Services.Productos;
using DistribuidoraLaVilla.Domain.Entities.Productos;
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
            return services;
        }
    }
}
