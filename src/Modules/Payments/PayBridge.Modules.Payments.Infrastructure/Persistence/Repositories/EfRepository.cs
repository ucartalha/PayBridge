using Microsoft.EntityFrameworkCore;
using PayBridge.BuildingBlocks.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence.Repositories
{
    internal sealed class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly PaymentsDbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public EfRepository(PaymentsDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public async Task AddAsync(
    TEntity entity,
    CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);

        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellation = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellation);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellation = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync(cancellation);
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(predicate);

            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellation = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellation);
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellation = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellation);
        }

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellation = default)
        {
            return await _dbSet.FindAsync([id], cancellation);
        }

        public async Task<IReadOnlyList<TEntity>> GetPagedAsync<TKey>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TKey>> orderBy, int pageNumber, int pageSize, bool ascending = true, CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0 )
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }
            if (pageSize<=0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (predicate is not null)
            {
                query = query.Where(predicate);
            }
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public void Remove(TEntity entity)
        {
           _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }
    }
}
