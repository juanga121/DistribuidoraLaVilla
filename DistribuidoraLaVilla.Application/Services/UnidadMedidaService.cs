using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Application.Services
{
    public class UnidadMedidaService(IGenericRepository<UnidadMedidaEntity, int> unidadMedida)
    {
        private readonly IGenericRepository<UnidadMedidaEntity, int> _unidadMedida = unidadMedida;

        public async Task<List<UnidadMedidaEntity>> ObtenerUnidadesMedidaAsync()
        {
            return await _unidadMedida.GetAllAsync();
        }
    }
}
