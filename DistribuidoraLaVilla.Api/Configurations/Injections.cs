using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Application.Services;
using DistribuidoraLaVilla.Application.Services.MateriaPrima;
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
            services.AddScoped<UnidadMedidaService>();
            return services;
        }
    }
}
