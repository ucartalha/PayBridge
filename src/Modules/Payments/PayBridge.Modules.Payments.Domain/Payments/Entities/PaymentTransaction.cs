using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Payments.Domain.Payments.Enums;
using PayBridge.Modules.Payments.Domain.Payments.Errors;

namespace PayBridge.Modules.Payments.Domain.Payments.Entities;

public class PaymentTransaction
{
    private PaymentTransaction()
    {
    }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public PaymentTransactionType Type { get; private set; }
    public PaymentTransactionStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public string ProviderCode { get; private set; } = default!;
    public string? ProviderTransactionId { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static PaymentTransaction CreatePending(
        Guid paymentId,
        PaymentTransactionType type,
        decimal amount,
        string providerCode)
    {
        ValidateBase(paymentId, amount, providerCode);

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Type = type,
            Status = PaymentTransactionStatus.Pending,
            Amount = amount,
            ProviderCode = providerCode,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static PaymentTransaction CreateSucceeded(
        Guid paymentId,
        PaymentTransactionType type,
        decimal amount,
        string providerCode,
        string? providerTransactionId)
    {
        ValidateBase(paymentId, amount, providerCode);

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Type = type,
            Status = PaymentTransactionStatus.Succeeded,
            Amount = amount,
            ProviderCode = providerCode,
            ProviderTransactionId = providerTransactionId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static PaymentTransaction CreateFailed(
        Guid paymentId,
        PaymentTransactionType type,
        decimal amount,
        string providerCode,
        string errorCode,
        string errorMessage)
    {
        ValidateBase(paymentId, amount, providerCode);

        if (string.IsNullOrWhiteSpace(errorCode))
            throw new BusinessException((int)PaymentTransactionErrorCode.ErrorCodeRequired);

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new BusinessException((int)PaymentTransactionErrorCode.ErrorMessageRequired);

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            Type = type,
            Status = PaymentTransactionStatus.Failed,
            Amount = amount,
            ProviderCode = providerCode,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
    public void MarkAsPending(
    string errorCode,
    string errorMessage)
    {
        ValidateError(errorCode, errorMessage);

        if (Status == PaymentTransactionStatus.Pending)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        if (Status is PaymentTransactionStatus.Succeeded or PaymentTransactionStatus.Failed)
        {
            return;
        }

        Status = PaymentTransactionStatus.Pending;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsSucceeded(string providerTransactionId)
    {
        if (Status == PaymentTransactionStatus.Succeeded)
        {
            return;
        }

        if (Status == PaymentTransactionStatus.Failed)
        {
            throw new BusinessException(
                (int)PaymentTransactionErrorCode.FailedTransactionCannotBeMarkedAsSucceeded);
        }

        if (string.IsNullOrWhiteSpace(providerTransactionId))
        {
            throw new BusinessException(
                (int)PaymentTransactionErrorCode.ProviderTransactionIdRequired);
        }

        ProviderTransactionId = providerTransactionId;
        ErrorCode = null;
        ErrorMessage = null;
        Status = PaymentTransactionStatus.Succeeded;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed(
        string errorCode,
        string errorMessage)
    {
        ValidateError(errorCode, errorMessage);

        if (Status == PaymentTransactionStatus.Failed)
        {
            return;
        }

        if (Status == PaymentTransactionStatus.Succeeded)
        {
            throw new BusinessException(
                (int)PaymentTransactionErrorCode.SucceededTransactionCannotBeMarkedAsFailed);
        }

        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Status = PaymentTransactionStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateError(
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new BusinessException(
                (int)PaymentTransactionErrorCode.ErrorCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new BusinessException(
                (int)PaymentTransactionErrorCode.ErrorMessageRequired);
        }
    }
    private static void ValidateBase(Guid paymentId, decimal amount, string providerCode)
    {
        if (paymentId == Guid.Empty)
            throw new BusinessException((int)PaymentTransactionErrorCode.PaymentIdRequired);

        if (amount <= 0)
            throw new BusinessException((int)PaymentTransactionErrorCode.AmountMustBeGreaterThanZero);

        if (string.IsNullOrWhiteSpace(providerCode))
            throw new BusinessException((int)PaymentTransactionErrorCode.ProviderCodeRequired);
    }
}