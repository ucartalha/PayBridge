using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;
using PayBridge.Modules.Merchants.Contracts.Merchants;
using PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;
using PayBridge.Modules.Payments.Application.Abstractions;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

internal sealed class PaymentActorResolver
    : IPaymentActorResolver
{
    private const int MerchantCodeRequired = 200001;
    private const int SectorNotActive = 200012;
    private const int MccNotActive = 200022;
    private const int MccRestricted = 200023;
    private const int MerchantNotFound = 200030;
    private const int MerchantNotActive = 200031;
    private const int MerchantSuspended = 200032;
    private const int MerchantClosed = 200033;
    private const int PaymentChannelNotEnabled = 200041;
    private const int ChannelAmountBelowMinimum = 200045;
    private const int ChannelAmountLimitExceeded = 200046;
    private const int PaymentChannelRequired = 200047;
    private const int PaymentChannelNotSupported = 200048;

    private readonly IMerchantReader _merchantReader;
    private readonly IIntegrationPaymentAccessService
        _integrationPaymentAccessService;

    public PaymentActorResolver(
        IMerchantReader merchantReader,
        IIntegrationPaymentAccessService
            integrationPaymentAccessService)
    {
        _merchantReader = merchantReader;
        _integrationPaymentAccessService =
            integrationPaymentAccessService;
    }

    public async Task<PaymentActorContext> ResolveAsync(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIntegrationIdentity(request);
        EnsureMerchantRequest(request);

        var normalizedChannel =
            MerchantPaymentChannels.NormalizeChannel(
                request.Channel.Trim());

        var merchant = await _merchantReader
            .GetPaymentAccessInfoByCodeAsync(
                request.MerchantCode.Trim(),
                normalizedChannel,
                cancellationToken);

        if (merchant is null)
        {
            throw new BusinessException(MerchantNotFound);
        }

        EnsureMerchantCanReceivePayment(
            merchant,
            request.Amount);

        var hasAccess =
            await _integrationPaymentAccessService
                .CanCreatePaymentAsync(
                    request.IntegrationClientId,
                    merchant.MerchantId,
                    request.Amount,
                    request.Currency,
                    cancellationToken);

        if (!hasAccess)
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode
                    .PaymentTargetNotAllowed);
        }

        return new PaymentActorContext(
            IntegrationClientId: request.IntegrationClientId,
            ClientCode: request.ClientCode,
            MerchantId: merchant.MerchantId,
            MerchantCode: merchant.MerchantCode,
            MerchantDisplayName: merchant.MerchantDisplayName,
            Channel: normalizedChannel);
    }

    private static void EnsureIntegrationIdentity(
        PaymentExecutionRequest request)
    {
        if (request.IntegrationClientId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.ClientCode))
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.InvalidToken);
        }
    }

    private static void EnsureMerchantRequest(
        PaymentExecutionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MerchantCode))
        {
            throw new BusinessException(
                MerchantCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Channel))
        {
            throw new BusinessException(
                PaymentChannelRequired);
        }

        if (!MerchantPaymentChannels.IsSupportedChannel(
                request.Channel))
        {
            throw new BusinessException(
                PaymentChannelNotSupported);
        }
    }

    private static void EnsureMerchantCanReceivePayment(
        MerchantPaymentAccessInfo merchant,
        decimal amount)
    {
        switch (merchant.MerchantStatus)
        {
            case MerchantStatusCode.Active:
                break;

            case MerchantStatusCode.Suspended:
                throw new BusinessException(
                    MerchantSuspended);

            case MerchantStatusCode.Closed:
                throw new BusinessException(
                    MerchantClosed);

            case MerchantStatusCode.Passive:
            default:
                throw new BusinessException(
                    MerchantNotActive);
        }

        if (!merchant.SectorIsActive)
        {
            throw new BusinessException(
                SectorNotActive);
        }

        if (!merchant.MccIsActive)
        {
            throw new BusinessException(
                MccNotActive);
        }

        if (merchant.MccIsRestricted)
        {
            throw new BusinessException(
                MccRestricted);
        }

        if (!merchant.ChannelIsEnabled)
        {
            throw new BusinessException(
                PaymentChannelNotEnabled);
        }

        if (amount < merchant.MinAmount)
        {
            throw new BusinessException(
                ChannelAmountBelowMinimum);
        }

        if (amount > merchant.MaxAmount)
        {
            throw new BusinessException(
                ChannelAmountLimitExceeded);
        }

        if (merchant.DailyAmountLimit.HasValue &&
            amount > merchant.DailyAmountLimit.Value)
        {
            throw new BusinessException(
                ChannelAmountLimitExceeded);
        }
    }
}