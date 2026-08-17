using Microsoft.AspNetCore.Mvc;

namespace PayBridge.Api.Authorization;

public sealed class IntegrationPaymentAuthorizeAttribute
    : TypeFilterAttribute
{
    public IntegrationPaymentAuthorizeAttribute(
        string requiredScope)
        : base(typeof(IntegrationPaymentAuthorizeFilter))
    {
        Arguments = new object[]
        {
            requiredScope
        };
    }
}