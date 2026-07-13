using PayBridge.BuildingBlocks.Persistence;

namespace PayBridge.Modules.Merchants.Application.Abstractions;

public interface IMerchantsUnitOfWork : IUnitOfWork
{
    IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class;
}