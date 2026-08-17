using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;

namespace PayBridge.Api.Authorization;

public sealed class IntegrationPaymentAuthorizeFilter
    : IAsyncActionFilter
{
    private readonly string _requiredScope;

    public IntegrationPaymentAuthorizeFilter(
        string requiredScope)
    {
        _requiredScope = requiredScope;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        EnsureScope(user);

        await next();
    }

    private void EnsureScope(
        System.Security.Claims.ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(_requiredScope))
        {
            return;
        }

        var scopes = user
            .FindAll(IntegrationAuthClaimNames.Scope)
            .SelectMany(claim => claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        if (!scopes.Contains(
                _requiredScope,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.InsufficientScope);
        }
    }
}