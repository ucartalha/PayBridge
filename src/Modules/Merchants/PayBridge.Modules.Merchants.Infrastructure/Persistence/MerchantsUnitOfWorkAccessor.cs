using System.Reflection;
using PayBridge.BuildingBlocks.Persistence;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence;

internal sealed class MerchantsUnitOfWorkAccessor : IUnitOfWorkAccessor
{
    private static readonly Assembly MerchantsApplicationAssembly =
        LoadApplicationAssemblyFromInfrastructureAssembly();

    private readonly MerchantsUnitOfWork _unitOfWork;

    public MerchantsUnitOfWorkAccessor(MerchantsUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork => _unitOfWork;

    public bool CanHandle(Type requestType)
    {
        return requestType.Assembly == MerchantsApplicationAssembly;
    }

    private static Assembly LoadApplicationAssemblyFromInfrastructureAssembly()
    {
        var infrastructureAssemblyName =
            typeof(MerchantsUnitOfWorkAccessor).Assembly.GetName().Name;

        if (string.IsNullOrWhiteSpace(infrastructureAssemblyName))
        {
            throw new InvalidOperationException(
                "Infrastructure assembly name could not be resolved.");
        }

        var applicationAssemblyName = infrastructureAssemblyName.Replace(
            ".Infrastructure",
            ".Application",
            StringComparison.Ordinal);

        if (applicationAssemblyName == infrastructureAssemblyName)
        {
            throw new InvalidOperationException(
                $"Infrastructure assembly name '{infrastructureAssemblyName}' does not follow expected module naming convention.");
        }

        return Assembly.Load(applicationAssemblyName);
    }
}