using Microsoft.Extensions.DependencyInjection;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

namespace PayBridge.Modules.Payments.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IPaymentOrchestrator, PaymentOrchestrator>();
        services.AddScoped<
           IProviderPaymentResultResolver,
           ProviderPaymentResultResolver>();
        return services;
    }
}