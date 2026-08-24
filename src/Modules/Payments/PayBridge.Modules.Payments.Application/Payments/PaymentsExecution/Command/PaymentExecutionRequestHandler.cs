using MediatR;
using PayBridge.Modules.Payments.Application.Abstractions;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

internal sealed class PaymentExecutionRequestHandler
    : IRequestHandler<
        PaymentExecutionRequest,
        PaymentExecutionResult>
{
    private readonly IPaymentOrchestrator _paymentOrchestrator;

    public PaymentExecutionRequestHandler(
        IPaymentOrchestrator paymentOrchestrator)
    {
        _paymentOrchestrator = paymentOrchestrator;
    }

    public Task<PaymentExecutionResult> Handle(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken)
    {
        return _paymentOrchestrator.ExecutePaymentAsync(
            request,
            cancellationToken);
    }
}