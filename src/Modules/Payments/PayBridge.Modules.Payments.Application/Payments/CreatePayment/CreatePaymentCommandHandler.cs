using PayBridge.BuildingBlocks.CQRS;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using PayBridge.Modules.Payments.Domain.Payments.Enums;

namespace PayBridge.Modules.Payments.Application.Payments.CreatePayment;

internal sealed class CreatePaymentCommandHandler
    : ICommandHandler<CreatePaymentCommand, CreatePaymentResult>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<PaymentTransaction> _transactionRepository;
    private readonly IRepository<IdempotencyRecord> _idempotencyRepository;

    public CreatePaymentCommandHandler(
        IRepository<Payment> paymentRepository,
        IRepository<PaymentTransaction> transactionRepository,
        IRepository<IdempotencyRecord> idempotencyRepository)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _idempotencyRepository = idempotencyRepository;
    }

    public async Task<CreatePaymentResult> Handle(
     CreatePaymentCommand command,
     CancellationToken cancellationToken)
    {

        var payment = Payment.Create(
            command.MerchantId,
            command.OrderId,
            command.Amount,
            command.Currency,
            command.ProviderCode);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var initialTransaction = PaymentTransaction.CreatePending(
            paymentId: payment.Id,
            type: PaymentTransactionType.Sale,
            amount: command.Amount,
            providerCode: command.ProviderCode
        );

        await _transactionRepository.AddAsync(initialTransaction, cancellationToken);

        return new CreatePaymentResult(payment.Id, payment.Status.ToString());
    }
}