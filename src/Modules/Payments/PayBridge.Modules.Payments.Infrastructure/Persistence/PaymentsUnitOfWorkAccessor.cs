using System.Reflection;
using PayBridge.BuildingBlocks.Persistence;

namespace PayBridge.Modules.Payments.Infrastructure.Persistence;

internal sealed class PaymentsUnitOfWorkAccessor : IUnitOfWorkAccessor
{
    private static readonly Assembly PaymentsApplicationAssembly =
        LoadApplicationAssemblyFromInfrastructureAssembly();

    private readonly PaymentsUnitOfWork _unitOfWork;

    public PaymentsUnitOfWorkAccessor(PaymentsUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork => _unitOfWork;

    public bool CanHandle(Type requestType)
    {
        return requestType.Assembly == PaymentsApplicationAssembly;
    }

    private static Assembly LoadApplicationAssemblyFromInfrastructureAssembly()
    {
        var infrastructureAssemblyName =
            typeof(PaymentsUnitOfWorkAccessor).Assembly.GetName().Name;

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