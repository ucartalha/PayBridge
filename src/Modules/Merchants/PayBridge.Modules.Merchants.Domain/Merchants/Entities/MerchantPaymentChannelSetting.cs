using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities
{
    public sealed class MerchantPaymentChannelSetting
    {
        public Guid Id { get; private set; }
        public Guid MerchantId { get; private set; }

        public MerchantPaymentChannel Channel { get; private set; }
        public bool IsEnabled { get;private set; }

        public decimal? MinAmount { get;private set; }
        public decimal? MaxAmount{ get;private set; }
        public decimal? DailyAmountLimit { get;private set; }

        public bool Require3DS { get;private set; }
        public bool AllowRefund { get;private set; }
        public bool AllowVoid { get;private set; }

        public DateTime CreatedAtUtc { get;private set; }
        public DateTime? EnabledAtUtc { get;private set; }
        public DateTime? DisabledAtUtc { get;private set; }


        public MerchantPaymentChannelSetting()
        {
        }

        public static MerchantPaymentChannelSetting Create(
            Guid merchantId,
            MerchantPaymentChannel channel,
            bool isEnabled = true,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            decimal? dailyAmountLimit = null,
            bool require3DS = false,
            bool allowRefund = true,
            bool allowVoid = true)
        {
            if (merchantId == Guid.Empty)
            {
                throw new BusinessException((int)MerchantErrorCode.MerchantIdRequired);
            }

            ValidateLimits(minAmount, maxAmount, dailyAmountLimit);
            return new MerchantPaymentChannelSetting
            {
                Id = Guid.NewGuid(),
                MerchantId = merchantId,
                Channel = channel,
                IsEnabled = isEnabled,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                DailyAmountLimit = dailyAmountLimit,
                Require3DS = require3DS,
                AllowRefund = allowRefund,
                AllowVoid = allowVoid,
                CreatedAtUtc = DateTime.UtcNow,
                EnabledAtUtc = isEnabled ? DateTime.UtcNow : (DateTime?)null
            };
        }
        public void Enable()
        {
            IsEnabled = true;
            EnabledAtUtc = DateTime.UtcNow;
            DisabledAtUtc = null;
        }
        public void Disable()
        {
            IsEnabled = false;
            DisabledAtUtc = DateTime.UtcNow;
        }

        public void ConfigureLimits(
            decimal? minAmount,
            decimal? maxAmount,
            decimal? dailyAmountLimit)
        {
            ValidateLimits(minAmount, maxAmount, dailyAmountLimit);
            MinAmount = minAmount;
            MaxAmount = maxAmount;
            DailyAmountLimit = dailyAmountLimit;
        }

        public void ConfigureCapabilities(
            bool require3DS,
            bool allowRefund,
            bool allowVoid)
        {
            Require3DS = require3DS;
            AllowRefund = allowRefund;
            AllowVoid = allowVoid;
        }

        public void EnsureCanProcess(decimal amount)
        {
            if (!IsEnabled)
            {
                throw new BusinessException((int)MerchantErrorCode.PaymentChannelNotEnabled);
            }
            if (MinAmount is not null && amount < MinAmount)
            {
                throw new BusinessException((int)MerchantErrorCode.ChannelAmountBelowMinimum);
            }

            if (MaxAmount is not null && amount > MaxAmount)
            {
                throw new BusinessException((int)MerchantErrorCode.ChannelAmountLimitExceeded);
            }
        }
        public void EnsureCanRefund()
        {
            if (!IsEnabled || !AllowRefund)
            {
                throw new BusinessException((int)MerchantErrorCode.PaymentChannelNotEnabled);
            }
        }
        public void EnsureCanVoid()
        {
            if (!IsEnabled || !AllowVoid)
            {
                throw new BusinessException((int)MerchantErrorCode.PaymentChannelNotEnabled);
            }
        }
        private static void ValidateLimits(
            decimal? minAmount,
            decimal? maxAmount,
            decimal? dailyAmountLimit)
        {
            if (minAmount != null && minAmount<=0)
            {
                throw new BusinessException((int)MerchantErrorCode.InvalidChannelMinAmount);
            }
            if (maxAmount != null && maxAmount<=0)
            {
                throw new BusinessException((int)MerchantErrorCode.InvalidChannelMaxAmount);
            }
            if (dailyAmountLimit != null && dailyAmountLimit <= 0)
            {
                throw new BusinessException((int)MerchantErrorCode.InvalidChannelDailyLimit);
            }
            if (minAmount!=null &&
                maxAmount != null &&
                minAmount > maxAmount)
            {
                throw new BusinessException((int)MerchantErrorCode.InvalidChannelMaxAmount);
            }
        }
    }
}
