namespace DistribuidoraLaVilla.Domain.Interfaces
{
    /// <summary>
    /// Abstracción de transacciones para operaciones multi-tabla.
    /// Implementado en Infrastructure usando EF Core DataContext.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>Indica si hay una transacción activa actualmente</summary>
        bool HasActiveTransaction { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
