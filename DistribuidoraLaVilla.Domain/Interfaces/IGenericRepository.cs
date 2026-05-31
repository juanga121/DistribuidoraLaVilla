using System.Linq.Expressions;

namespace DistribuidoraLaVilla.Domain.Interfaces
{
    public interface IGenericRepository<T, TKey> where TKey : notnull
    {
        Task CreateAsync(T entity);
        Task DeleteAsync(TKey entity);
        Task<T?> FindByIdAsync(TKey id);
        Task<List<T>> GetAllAsync();
        List<T> GetByFilter(Expression<Func<T, bool>> filter);
        IEnumerable<T> GetByFilterOrdered(Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy, bool? isDesc = true);
        Task UpdateAsync(T entity);
    }
}
