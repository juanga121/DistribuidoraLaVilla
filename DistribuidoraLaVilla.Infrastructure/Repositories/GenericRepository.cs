using DistribuidoraLaVilla.Application.Repositories;
using DistribuidoraLaVilla.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraLaVilla.Infrastructure.Repositories
{
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey>
    where T : class
    where TKey : notnull
    {
        private readonly DataContext _efContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DataContext efContext)
        {
            _efContext = efContext;
            _dbSet = _efContext.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _efContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(TKey entity)
        {
            T? result = await FindByIdAsync(entity) ??
                throw new Exception($"{nameof(T)} Not Found");
            _dbSet.Remove(result);
            await _efContext.SaveChangesAsync();
        }
        public async Task<T?> FindByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync() ??
                throw new Exception($"{nameof(T)} List Not Found");
        }
        public List<T> GetByFilter(Expression<Func<T, bool>> filter)
        {
            return _dbSet.Where(filter).ToList() ??
                       throw new Exception($"{typeof(T).Name} no encontrado");
        }
        public IEnumerable<T> GetByFilterOrdered(Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy, bool? isDesc = true)
        {
            if (isDesc == false)
                return _dbSet.Where(predicate).OrderBy(orderBy);
            else
                return _dbSet.Where(predicate).OrderByDescending(orderBy);
        }
        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
