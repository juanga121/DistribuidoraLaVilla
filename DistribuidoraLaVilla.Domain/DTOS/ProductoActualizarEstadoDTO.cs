using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class ProductoActualizarEstadoDTO
    {
        public int Id { get; set; }
        public int EstadoNuevo { get; set; }
    }
}
