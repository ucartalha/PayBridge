using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.CreatePayment
{
    public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.MerchantId).NotEmpty().WithMessage("Üye işyeri bilgisi boş bırakılmamalı");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("Sipariş numarası zorunludur.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Lütfen geçerli bir tutar giriniz.");
            RuleFor(x => x.Currency).NotEmpty().WithMessage("Para birimi boş bırakılamaz.").
                Length(3).WithMessage("Para birimi 3 karakterden oluşmalı");
            RuleFor(x => x.ProviderCode).NotEmpty().WithMessage("Sağlayıcı kodu boş bırakılamaz.");
        }
    }
}
