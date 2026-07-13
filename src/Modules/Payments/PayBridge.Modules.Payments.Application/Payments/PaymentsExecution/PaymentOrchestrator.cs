using MediatR;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Application.Payments.CompletePayment;
using PayBridge.Modules.Payments.Application.Payments.CreatePayment;
using PayBridge.Modules.Providers.Contracts;

namespace PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

public sealed class PaymentOrchestrator : IPaymentOrchestrator
{
    private readonly ISender _sender;
    private readonly IPaymentProviderFactory _providerFactory;
    private readonly IProviderPaymentResultResolver _providerResultResolver;

    public PaymentOrchestrator(
        ISender sender,
        IPaymentProviderFactory providerFactory,
        IProviderPaymentResultResolver providerResultResolver)
    {
        _sender = sender;
        _providerFactory = providerFactory;
        _providerResultResolver = providerResultResolver;
    }

    public async Task<PaymentExecutionResult> ExecutePaymentAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var pendingResult = await CreatePendingPaymentAsync(
            command,
            cancellationToken);

        var provider = _providerFactory.Resolve(command.ProviderCode);

        var providerResult = await ResolveProviderResultAsync(
            provider,
            pendingResult.PaymentId,
            command,
            cancellationToken);

        var completeResult = await CompletePaymentAsync(
            pendingResult.PaymentId,
            providerResult);

        return ToExecutionResult(completeResult);
    }

    private async Task<CreatePaymentResult> CreatePendingPaymentAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(
            command,
            cancellationToken);
    }

    private async Task<ProviderFinalResult> ResolveProviderResultAsync(
        IPaymentProvider provider,
        Guid paymentId,
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var providerRequest = new ProviderChargeRequest(
            PaymentId: paymentId,
            OrderId: command.OrderId,
            Amount: command.Amount,
            Currency: command.Currency,
            IdempotencyKey: paymentId.ToString("N"));

        return await _providerResultResolver.ResolveAsync(
            provider,
            providerRequest,
            cancellationToken);
    }

    private async Task<CompletePaymentResult> CompletePaymentAsync(
        Guid paymentId,
        ProviderFinalResult providerResult)
    {
        var completeCommand = new CompletePaymentCommand(
            PaymentId: paymentId,
            ProviderState: providerResult.State,
            ProviderTransactionId: providerResult.ProviderTransactionId,
            ErrorCode: providerResult.ErrorCode,
            ErrorMessage: providerResult.ErrorMessage);

        return await _sender.Send(
            completeCommand,
            CancellationToken.None);
    }

    private static PaymentExecutionResult ToExecutionResult(
        CompletePaymentResult completeResult)
    {
        return new PaymentExecutionResult(
            PaymentId: completeResult.PaymentId,
            Status: completeResult.Status,
            ProviderTransactionId: completeResult.ProviderTransactionId,
            ErrorCode: completeResult.ErrorCode,
            ErrorMessage: completeResult.ErrorMessage);
    }
}