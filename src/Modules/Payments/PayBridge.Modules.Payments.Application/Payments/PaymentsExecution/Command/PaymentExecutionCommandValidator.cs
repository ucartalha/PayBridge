using FluentValidation;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

public sealed class PaymentExecutionRequestValidator
    : AbstractValidator<PaymentExecutionRequest>
{
    public PaymentExecutionRequestValidator()
    {
        RuleFor(x => x.IntegrationClientId)
            .NotEmpty();

        RuleFor(x => x.ClientCode)
            .NotEmpty();

        RuleFor(x => x.MerchantCode)
            .NotEmpty();

        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.ProviderCode)
            .NotEmpty();

        RuleFor(x => x.Channel)
            .NotEmpty();
    }
}