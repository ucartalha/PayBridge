using MediatR;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Results;
using PayBridge.Modules.Payments.Application.Abstractions;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

internal sealed class PaymentExecutionRequestHandler
    : IRequestHandler<
        PaymentExecutionRequest,
        Result<PaymentExecutionResult>>
{
    private readonly IPaymentOrchestrator
        _paymentOrchestrator;

    public PaymentExecutionRequestHandler(
        IPaymentOrchestrator paymentOrchestrator)
    {
        _paymentOrchestrator =
            paymentOrchestrator;
    }

    public async Task<Result<PaymentExecutionResult>>
        Handle(
            PaymentExecutionRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _paymentOrchestrator
                    .ExecutePaymentAsync(
                        request,
                        cancellationToken);

            return Result<PaymentExecutionResult>
                .Success(result);
        }
        catch (BusinessException exception)
        {
            return Result<PaymentExecutionResult>
                .BusinessFailure(
                    exception.ErrorCode);
        }
    }
}