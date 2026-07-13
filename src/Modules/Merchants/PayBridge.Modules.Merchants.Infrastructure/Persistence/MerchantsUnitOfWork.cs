using Microsoft.EntityFrameworkCore.Storage;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.Modules.Merchants.Application.Abstractions;
using PayBridge.Modules.Merchants.Infrastructure.Persistence.Repositories;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence;

internal sealed class MerchantsUnitOfWork : IMerchantsUnitOfWork
{
    private readonly MerchantsDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    private IDbContextTransaction? _currentTransaction;

    public MerchantsUnitOfWork(MerchantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (_repositories.TryGetValue(entityType, out var repository))
        {
            return (IRepository<TEntity>)repository;
        }

        var createdRepository = new EfRepository<TEntity>(_dbContext);

        _repositories.Add(entityType, createdRepository);

        return createdRepository;
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
    }
}