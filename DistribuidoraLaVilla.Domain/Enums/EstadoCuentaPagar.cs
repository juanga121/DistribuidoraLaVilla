namespace DistribuidoraLaVilla.Domain.Enums
{
    /// <summary>
    /// Estados de las cuentas por pagar.
    /// Los estados 1 a 3 se persisten en la entidad. ParcialmentePagada y Vencida
    /// son estados derivados que se evalúan en servidor al mapear el DTO, siguiendo
    /// el mismo approach que CxC usa para Vencida.
    /// </summary>
    public enum EstadoCuentaPagar
    {
        Pendiente = 1,
        Pagada = 2,
        Cancelada = 3,
        ParcialmentePagada = 4,
        Vencida = 5
    }
}