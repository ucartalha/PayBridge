namespace PayBridge.Modules.Payments.Domain.Payments.Errors;

public enum PaymentErrorCode
{
    MerchantIdRequired = 100001,
    OrderIdRequired = 100002,
    AmountMustBeGreaterThanZero = 100003,
    CurrencyRequired = 100004,
    ProviderCodeRequired = 100005,


    OnlyPendingPaymentCanBeMarkedAsProcessing = 100010,
    OnlyPendingOrProcessingPaymentCanBeMarkedAsSucceeded = 100011,
    ProviderTransactionIdRequired = 100012,
    SucceededPaymentCannotBeMarkedAsFailed = 100013,

    OnlySucceededPaymentCanBeVoided = 100020,
    RefundedPaymentCannotBeVoided = 100021,

    OnlySucceededOrPartiallyRefundedPaymentCanBeRefunded = 100030,
    RefundAmountMustBeGreaterThanZero = 100031,
    RefundAmountExceedsRefundableAmount = 100032,

    OnlyPendingOrProcessingPaymentCanBeExpired = 100040,

    PaymentNotFound = 100051
}