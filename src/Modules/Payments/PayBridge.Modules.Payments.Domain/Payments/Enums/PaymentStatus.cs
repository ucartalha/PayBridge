
namespace PayBridge.Modules.Payments.Domain.Payments.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Voided = 5,
        PartiallyRefunded = 6,
        Refunded = 7,
        Expired = 8
    }
}
