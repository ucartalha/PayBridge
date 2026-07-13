using Microsoft.EntityFrameworkCore.Storage;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence
{
    internal sealed class PaymentsUnitOfWork : IPaymentsUnitOfWork
    {
        private readonly PaymentsDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction _currentTransaction;

        public PaymentsUnitOfWork(PaymentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IRepository<TEntity>) repository;
            }
            var createdRepository = new EfRepository<TEntity>(_dbContext);
            _repositories.Add(entityType, createdRepository);
            return createdRepository;
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null)
            {
                return;
            }
            _currentTransaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {
                return;
            }
            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {
                return;
            }

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();

            _currentTransaction = null;

        }


        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
