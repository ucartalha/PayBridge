
namespace PayBridge.Modules.Payments.Domain.Payments.Enums
{
    public enum PaymentTransactionType
    {
        Sale = 1,
        Void = 2,
        Refund = 3,
        WebhookUpdate = 4
    }
}
