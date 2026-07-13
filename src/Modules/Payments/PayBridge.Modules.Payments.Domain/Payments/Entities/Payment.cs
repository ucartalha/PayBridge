using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Payments.Domain.Payments.Enums;
using PayBridge.Modules.Payments.Domain.Payments.Errors;

namespace PayBridge.Modules.Payments.Domain.Payments.Entities;

public class Payment
{
    private Payment()
    {
    }

    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public string OrderId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public string ProviderCode { get; private set; } = default!;
    public string? ProviderTransactionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public decimal RefundableAmount => Amount - RefundedAmount;

    public static Payment Create(
        Guid merchantId,
        string orderId,
        decimal amount,
        string currency,
        string providerCode)
    {
        if (merchantId == Guid.Empty)
            throw new BusinessException((int)PaymentErrorCode.MerchantIdRequired);

        if (string.IsNullOrWhiteSpace(orderId))
            throw new BusinessException((int)PaymentErrorCode.OrderIdRequired);

        if (amount <= 0)
            throw new BusinessException((int)PaymentErrorCode.AmountMustBeGreaterThanZero);

        if (string.IsNullOrWhiteSpace(currency))
            throw new BusinessException((int)PaymentErrorCode.CurrencyRequired);

        if (string.IsNullOrWhiteSpace(providerCode))
            throw new BusinessException((int)PaymentErrorCode.ProviderCodeRequired);

        return new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            OrderId = orderId,
            Amount = amount,
            RefundedAmount = 0,
            Currency = currency,
            ProviderCode = providerCode,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        if (Status == PaymentStatus.Processing)
        {
            return;
        }

        if (Status != PaymentStatus.Pending)
            throw new BusinessException((int)PaymentErrorCode.OnlyPendingPaymentCanBeMarkedAsProcessing);

        Status = PaymentStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;
    }


    public void MarkAsSucceeded(string providerTransactionId)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return;
        }

        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new BusinessException((int)PaymentErrorCode.OnlyPendingOrProcessingPaymentCanBeMarkedAsSucceeded);

        if (string.IsNullOrWhiteSpace(providerTransactionId))
            throw new BusinessException((int)PaymentErrorCode.ProviderTransactionIdRequired);

        ProviderTransactionId = providerTransactionId;
        Status = PaymentStatus.Succeeded;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == PaymentStatus.Failed)
        {
            return;
        }

        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new BusinessException((int)PaymentErrorCode.SucceededPaymentCannotBeMarkedAsFailed); // bu hata mesajını daha sonra düzelteceğim

        Status = PaymentStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Void()
    {
        if (Status != PaymentStatus.Succeeded)
            throw new BusinessException((int)PaymentErrorCode.OnlySucceededPaymentCanBeVoided);

        if (RefundedAmount > 0)
            throw new BusinessException((int)PaymentErrorCode.RefundedPaymentCannotBeVoided);

        Status = PaymentStatus.Voided;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Refund(decimal amount)
    {
        if (Status != PaymentStatus.Succeeded && Status != PaymentStatus.PartiallyRefunded)
            throw new BusinessException((int)PaymentErrorCode.OnlySucceededOrPartiallyRefundedPaymentCanBeRefunded);

        if (amount <= 0)
            throw new BusinessException((int)PaymentErrorCode.RefundAmountMustBeGreaterThanZero);

        if (amount > RefundableAmount)
            throw new BusinessException((int)PaymentErrorCode.RefundAmountExceedsRefundableAmount);

        RefundedAmount += amount;

        Status = RefundedAmount == Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new BusinessException((int)PaymentErrorCode.OnlyPendingOrProcessingPaymentCanBeExpired);

        Status = PaymentStatus.Expired;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}