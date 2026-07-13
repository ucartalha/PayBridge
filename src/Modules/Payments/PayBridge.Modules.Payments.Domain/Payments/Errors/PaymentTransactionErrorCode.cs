namespace PayBridge.Modules.Payments.Domain.Payments.Errors;

public enum PaymentTransactionErrorCode
{
    PaymentIdRequired = 110001,
    AmountMustBeGreaterThanZero = 110002,
    ProviderCodeRequired = 110003,
    ErrorCodeRequired = 110004,
    ErrorMessageRequired = 110005,
    ProviderTransactionIdRequired = 110006,
    FailedTransactionCannotBeMarkedAsSucceeded = 110007,
    SucceededTransactionCannotBeMarkedAsFailed = 110008
}