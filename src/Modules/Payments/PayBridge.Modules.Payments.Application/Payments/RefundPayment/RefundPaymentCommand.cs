using PayBridge.BuildingBlocks.CQRS;

namespace PayBridge.Modules.Payments.Application.Payments.RefundPayment;
public sealed record RefundPaymentCommand(string MerchantCode, string OrderId, decimal Amount) : ICommand<RefundPaymentResult>;

