using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities;

public sealed class MerchantProviderAccount
{
    public Guid Id { get; private set; }

    public Guid MerchantId { get; private set; }

    public string ProviderCode { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public bool AllowECommerce { get; private set; }
    public bool AllowPhysicalPos { get; private set; }
    public bool AllowRefund { get; private set; }

    public int Priority { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ActivatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    private MerchantProviderAccount()
    {
    }

    public static MerchantProviderAccount Create(
        Guid merchantId,
        string providerCode,
        int priority)
    {
        if (merchantId == Guid.Empty)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantIdRequired);
        }

        if (string.IsNullOrWhiteSpace(providerCode))
        {
            throw new BusinessException((int)MerchantErrorCode.ProviderCodeRequired);
        }
        if (priority<0)
        {
            throw new BusinessException((int)MerchantErrorCode.InvalidProviderAccountPriority);
        }

        return new MerchantProviderAccount
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            ProviderCode = providerCode.Trim(),
            Priority = priority,
            IsActive = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        IsActive = true;
        ActivatedAtUtc = DateTime.UtcNow;
        DeactivatedAtUtc = null;
    }

    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAtUtc = DateTime.UtcNow;
    }

    public void ConfigureChannels(
        bool allowECommerce,
        bool allowPhysicalPos
        )
    {
        AllowECommerce = allowECommerce;
        AllowPhysicalPos = allowPhysicalPos;
    }

    public void ChangePriority(int priority)
    {
        Priority = priority;
    }

    public void EnsureCanProcess(MerchantPaymentChannel channel)
    {
        if (!IsActive)
        {
            throw new BusinessException((int)MerchantErrorCode.ProviderAccountNotActive);
        }

        switch (channel)
        {
            case MerchantPaymentChannel.ECommerce when !AllowECommerce:
                throw new BusinessException((int)MerchantErrorCode.ProviderAccountChannelNotAllowed);

            case MerchantPaymentChannel.PhysicalPos when !AllowPhysicalPos:
                throw new BusinessException((int)MerchantErrorCode.ProviderAccountChannelNotAllowed);

            case MerchantPaymentChannel.Wallet:
                throw new BusinessException((int)MerchantErrorCode.ProviderAccountChannelNotAllowed);
        }
    }

    public void EnsureCanRefund()
    {
        if (!IsActive)
        {
            throw new BusinessException((int)MerchantErrorCode.ProviderAccountNotActive);
        }

        if (!AllowRefund)
        {
            throw new BusinessException((int)MerchantErrorCode.ProviderAccountRefundNotAllowed);
        }
    }

    public void ConfigureRefund(bool allowRefund)
    {
        AllowRefund = allowRefund;
    }
}