namespace DistribuidoraLaVilla.Domain.Interfaces
{
    /// <summary>
    /// Abstracción de transacciones para operaciones multi-tabla.
    /// Implementado en Infrastructure usando EF Core DataContext.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
