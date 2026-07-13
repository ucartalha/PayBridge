using System.Linq.Expressions;

namespace PayBridge.BuildingBlocks.Persistence
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellation = default);
        Task AddAsync(TEntity entity, CancellationToken cancellation = default);
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellation = default);
        Task<IReadOnlyList<TEntity>> FindAsync<TKey>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TKey>> orderBy,
            bool ascending = true,
            CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellation = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellation = default);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellation = default);
        Task<IReadOnlyList<TEntity>> GetPagedAsync<TKey>(
            Expression<Func<TEntity, bool>>? predicate,
            Expression<Func<TEntity, TKey>> orderBy,
            int pageNumber,
            int pageSize,
            bool ascending = true,
            CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
