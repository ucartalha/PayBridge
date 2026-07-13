using PayBridge.BuildingBlocks.CQRS;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using PayBridge.Modules.Payments.Domain.Payments.Enums;
using PayBridge.Modules.Payments.Domain.Payments.Errors;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Payments.Application.Payments.CompletePayment;

internal sealed class CompletePaymentCommandHandler
    : ICommandHandler<CompletePaymentCommand, CompletePaymentResult>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<PaymentTransaction> _paymentTransactionRepository;

    public CompletePaymentCommandHandler(
        IRepository<Payment> paymentRepository,
        IRepository<PaymentTransaction> paymentTransactionRepository)
    {
        _paymentRepository = paymentRepository;
        _paymentTransactionRepository = paymentTransactionRepository;
    }

    public async Task<CompletePaymentResult> Handle(
        CompletePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            command.PaymentId,
            cancellationToken);

        if (payment is null)
        {
            throw new BusinessException(
                (int)PaymentErrorCode.PaymentNotFound);
        }

        var paymentTransaction = await _paymentTransactionRepository.FirstOrDefaultAsync(
            transaction =>
                transaction.PaymentId == command.PaymentId &&
                transaction.Type == PaymentTransactionType.Sale,
            cancellationToken);

        if (paymentTransaction is null)
        {
            throw new BusinessException(
                (int)PaymentErrorCode.PaymentNotFound);
        }

        if (IsTerminalStatus(payment.Status))
        {
            return ToResult(payment, paymentTransaction);
        }

        ApplyProviderState(
            payment,
            paymentTransaction,
            command);

        return ToResult(payment, paymentTransaction);
    }

    private static void ApplyProviderState(
        Payment payment,
        PaymentTransaction paymentTransaction,
        CompletePaymentCommand command)
    {
        switch (command.ProviderState)
        {
            case ProviderPaymentState.Succeeded:
                MarkAsSucceeded(
                    payment,
                    paymentTransaction,
                    command.ProviderTransactionId);
                break;

            case ProviderPaymentState.Failed:
                MarkAsFailed(
                    payment,
                    paymentTransaction,
                    command.ErrorCode,
                    command.ErrorMessage);
                break;

            case ProviderPaymentState.StillProcessing:
                MarkAsStillProcessing(
                    payment,
                    paymentTransaction,
                    command.ErrorCode,
                    command.ErrorMessage);
                break;

            default:
                MarkAsStillProcessing(
                    payment,
                    paymentTransaction,
                    "PROVIDER_UNKNOWN_STATE",
                    $"Unknown provider state: {command.ProviderState}");
                break;
        }
    }

    private static void MarkAsSucceeded(
        Payment payment,
        PaymentTransaction paymentTransaction,
        string? providerTransactionId)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
        {
            throw new BusinessException(
                (int)PaymentErrorCode.ProviderTransactionIdRequired);
        }

        payment.MarkAsSucceeded(providerTransactionId);
        paymentTransaction.MarkAsSucceeded(providerTransactionId);
    }

    private static void MarkAsFailed(
        Payment payment,
        PaymentTransaction paymentTransaction,
        string? errorCode,
        string? errorMessage)
    {
        payment.MarkAsFailed();

        paymentTransaction.MarkAsFailed(
            errorCode ?? "PROVIDER_PAYMENT_FAILED",
            errorMessage ?? "Provider payment failed.");
    }

    private static void MarkAsStillProcessing(
        Payment payment,
        PaymentTransaction paymentTransaction,
        string? errorCode,
        string? errorMessage)
    {
        payment.MarkAsProcessing();

        paymentTransaction.MarkAsPending(
            errorCode ?? "PROVIDER_STILL_PROCESSING",
            errorMessage ?? "Provider payment status is still processing.");
    }

    private static bool IsTerminalStatus(PaymentStatus status)
    {
        return status is PaymentStatus.Succeeded
            or PaymentStatus.Failed
            or PaymentStatus.Voided
            or PaymentStatus.PartiallyRefunded
            or PaymentStatus.Refunded
            or PaymentStatus.Expired;
    }

    private static CompletePaymentResult ToResult(
        Payment payment,
        PaymentTransaction paymentTransaction)
    {
        return new CompletePaymentResult(
            PaymentId: payment.Id,
            Status: payment.Status.ToString(),
            ProviderTransactionId: payment.ProviderTransactionId,
            ErrorCode: paymentTransaction.ErrorCode,
            ErrorMessage: paymentTransaction.ErrorMessage);
    }
}