using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities;

public class Merchant
{
    private Merchant()
    {
    }

    public Guid Id { get; private set; }

    public string MerchantCode { get; private set; } = default!;
    public string LegalName { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;

    public string TaxNumber { get; private set; } = default!;
    public string? TaxOffice { get; private set; }

    public Guid SectorId { get; private set; }
    public Guid MccId { get; private set; }

    public MerchantStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ActivatedAtUtc { get; private set; }
    public DateTime? SuspendedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    public static Merchant Create(
        string merchantCode,
        string legalName,
        string displayName,
        string taxNumber,
        string? taxOffice,
        Guid sectorId,
        Guid mccId)
    {
        if (string.IsNullOrWhiteSpace(merchantCode))
            throw new BusinessException((int)MerchantErrorCode.MerchantCodeRequired);

        if (string.IsNullOrWhiteSpace(legalName))
            throw new BusinessException((int)MerchantErrorCode.LegalNameRequired);

        if (string.IsNullOrWhiteSpace(displayName))
            throw new BusinessException((int)MerchantErrorCode.DisplayNameRequired);

        if (string.IsNullOrWhiteSpace(taxNumber))
            throw new BusinessException((int)MerchantErrorCode.TaxNumberRequired);

        if (sectorId == Guid.Empty)
            throw new BusinessException((int)MerchantErrorCode.SectorCodeRequired);
        
        if (mccId== Guid.Empty)
            throw new BusinessException((int)MerchantErrorCode.MccCodeRequired);

        return new Merchant
        {
            Id = Guid.NewGuid(),
            MerchantCode = merchantCode.Trim(),
            LegalName = legalName.Trim(),
            DisplayName = displayName.Trim(),
            TaxNumber = taxNumber.Trim(),
            TaxOffice  = string.IsNullOrWhiteSpace(taxOffice) ? null : taxOffice.Trim(),
            SectorId = sectorId,
            MccId = mccId,
            Status = MerchantStatus.Passive,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        if (Status == MerchantStatus.Closed)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantClosed);
        }
        Status = MerchantStatus.Active;
        ActivatedAtUtc = DateTime.UtcNow;
        SuspendedAtUtc= null;
    }

    public void Suspend() 
    {
        if (Status == MerchantStatus.Closed)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantClosed);
        }
        Status = MerchantStatus.Suspended;
        SuspendedAtUtc = DateTime.UtcNow;
    }
    public void Passive()
    {
        if (Status == MerchantStatus.Closed)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantClosed);
        }
        Status = MerchantStatus.Passive;
    }

    public void EnsureActive() 
    {
        if (Status == MerchantStatus.Active)
        {
            return;
        }
        if (Status == MerchantStatus.Suspended)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantSuspended);
        }
        if (Status == MerchantStatus.Closed)
        {
            throw new BusinessException((int)MerchantErrorCode.MerchantClosed);
        }
        Status = MerchantStatus.Active;
        ActivatedAtUtc= DateTime.UtcNow;
    }
}