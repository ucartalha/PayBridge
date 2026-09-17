using PayBridge.BuildingBlocks.CQRS;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using PayBridge.Modules.Payments.Domain.Payments.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.RefundPayment
{
    public class RefundPaymentCommandHandler : ICommandHandler<RefundPaymentCommand, RefundPaymentResult>
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<PaymentTransaction> _transactionRepository;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<MerchantProviderAccount> _mProviderAccount;

        public RefundPaymentCommandHandler(IRepository<Payment> paymentRepository,
            IRepository<PaymentTransaction> transactionRepository, IRepository<Merchant> merchantRepository,
            IRepository<MerchantProviderAccount> mProviderAccount)
        {
            _paymentRepository = paymentRepository;
            _transactionRepository = transactionRepository;
            _merchantRepository = merchantRepository;
            _mProviderAccount = mProviderAccount;
        }

        public async Task<RefundPaymentResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            var merchant = await _merchantRepository.FirstOrDefaultAsync(
                x => x.MerchantCode == request.MerchantCode && x.Status == Merchants.Domain.Merchants.Enums.MerchantStatus.Active);
            if (merchant is null)
            {
                throw new BusinessException((int)PaymentErrorCode.MerchantIdRequired);
            }
            var merchantProvider = _mProviderAccount.FirstOrDefaultAsync(x => x.MerchantId == merchant.Id && x.AllowRefund == true);

            var payment = await _paymentRepository.FirstOrDefaultAsync(x=>x.MerchantId ==merchant.Id && x.OrderId == request.OrderId);
            if (payment is null) {
                throw new BusinessException((int)PaymentErrorCode.PaymentNotFound);
            }
            var transaction = await _transactionRepository.FirstOrDefaultAsync(x => x.PaymentId == payment.Id && x.Type == Domain.Payments.Enums.PaymentTransactionType.Sale);
            if (transaction is null)
            {
                throw new BusinessException((int)PaymentErrorCode.PaymentNotFound);
            }
            




            return null;
        }
    }
}
