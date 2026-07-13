using PayBridge.BuildingBlocks.Persistence;

namespace PayBridge.BuildingBlocks.CQRS;

internal sealed class DefaultUnitOfWorkResolver : IUnitOfWorkResolver
{
    private readonly IEnumerable<IUnitOfWorkAccessor> _accessors;

    public DefaultUnitOfWorkResolver(IEnumerable<IUnitOfWorkAccessor> accessors)
    {
        _accessors = accessors;
    }

    public IUnitOfWork? Resolve(Type requestType)
    {
        return _accessors
            .FirstOrDefault(accessor => accessor.CanHandle(requestType))
            ?.UnitOfWork;
    }
}