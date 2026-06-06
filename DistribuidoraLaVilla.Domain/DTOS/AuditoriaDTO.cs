namespace DistribuidoraLaVilla.Domain.DTOS
{
    public class AuditoriaDTO
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public string? IdEntidad { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class AuditoriaListDTO
    {
        public List<AuditoriaDTO> Registros { get; set; } = new();
        public int Total { get; set; }
    }
}
