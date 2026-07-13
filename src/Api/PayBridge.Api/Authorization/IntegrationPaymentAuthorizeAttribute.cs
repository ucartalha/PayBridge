using Microsoft.AspNetCore.Mvc;

namespace PayBridge.Api.Authorization;

public sealed class IntegrationPaymentAuthorizeAttribute : TypeFilterAttribute
{
    public IntegrationPaymentAuthorizeAttribute(
        string requiredScope,
        bool requirePaymentAccess = true)
        : base(typeof(IntegrationPaymentAuthorizeFilter))
    {
        Arguments = new object[]
        {
            requiredScope,
            requirePaymentAccess
        };
    }
}