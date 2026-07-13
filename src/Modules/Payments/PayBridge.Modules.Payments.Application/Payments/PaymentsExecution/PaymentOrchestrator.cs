using MediatR;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Application.Payments.CompletePayment;
using PayBridge.Modules.Payments.Application.Payments.CreatePayment;
using PayBridge.Modules.Providers.Contracts;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

public sealed class PaymentOrchestrator
    : IPaymentOrchestrator
{
    private readonly ISender _sender;
    private readonly IPaymentProviderFactory _providerFactory;
    private readonly IProviderPaymentResultResolver
        _providerResultResolver;
    private readonly IPaymentActorResolver
        _paymentActorResolver;
    private readonly IProviderCredentialResolver
        _providerCredentialResolver;

    public PaymentOrchestrator(
        ISender sender,
        IPaymentProviderFactory providerFactory,
        IProviderPaymentResultResolver providerResultResolver,
        IPaymentActorResolver paymentActorResolver,
        IProviderCredentialResolver providerCredentialResolver)
    {
        _sender = sender;
        _providerFactory = providerFactory;
        _providerResultResolver = providerResultResolver;
        _paymentActorResolver = paymentActorResolver;
        _providerCredentialResolver =
            providerCredentialResolver;
    }

    public async Task<PaymentExecutionResult>
        ExecutePaymentAsync(
            PaymentExecutionRequest request,
            CancellationToken cancellationToken = default)
    {
        var actor = await _paymentActorResolver.ResolveAsync(
            request,
            cancellationToken);

        var provider = _providerFactory.Resolve(
            request.ProviderCode);

        var credential =
            await _providerCredentialResolver.ResolveAsync(
                actor.MerchantId,
                request.ProviderCode,
                actor.Channel,
                cancellationToken);

        var createCommand = new CreatePaymentCommand(
            MerchantId: actor.MerchantId,
            OrderId: request.OrderId,
            Amount: request.Amount,
            Currency: request.Currency,
            ProviderCode: request.ProviderCode);

        var pendingResult = await _sender.Send(
            createCommand,
            cancellationToken);

        var chargeRequest = new ProviderChargeRequest(
            PaymentId: pendingResult.PaymentId,
            OrderId: request.OrderId,
            Amount: request.Amount,
            Currency: request.Currency,
            IdempotencyKey:
                pendingResult.PaymentId.ToString("N"),
            Credential: credential);

        var providerResult =
            await _providerResultResolver.ResolveAsync(
                provider,
                chargeRequest,
                cancellationToken);

        var completeCommand =
            new CompletePaymentCommand(
                PaymentId: pendingResult.PaymentId,
                ProviderState: providerResult.State,
                ProviderTransactionId:
                    providerResult.ProviderTransactionId,
                ErrorCode: providerResult.ErrorCode,
                ErrorMessage: providerResult.ErrorMessage);

        var completeResult = await _sender.Send(
            completeCommand,
            CancellationToken.None);

        return new PaymentExecutionResult(
            PaymentId: completeResult.PaymentId,
            Status: completeResult.Status,
            ProviderTransactionId:
                completeResult.ProviderTransactionId,
            ErrorCode: completeResult.ErrorCode,
            ErrorMessage: completeResult.ErrorMessage);
    }
}