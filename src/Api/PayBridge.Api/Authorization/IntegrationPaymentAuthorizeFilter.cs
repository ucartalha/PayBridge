using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;
using System.Reflection;

namespace PayBridge.Api.Authorization;

public sealed class IntegrationPaymentAuthorizeFilter : IAsyncActionFilter
{
    private readonly string _requiredScope;
    private readonly bool _requirePaymentAccess;
    private readonly IIntegrationPaymentAccessService _paymentAccessService;

    public IntegrationPaymentAuthorizeFilter(
        string requiredScope,
        bool requirePaymentAccess,
        IIntegrationPaymentAccessService paymentAccessService)
    {
        _requiredScope = requiredScope;
        _requirePaymentAccess = requirePaymentAccess;
        _paymentAccessService = paymentAccessService;
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

        if (_requirePaymentAccess)
        {
            await EnsurePaymentAccessAsync(
                context,
                context.HttpContext.RequestAborted);
        }

        await next();
    }

    private void EnsureScope(System.Security.Claims.ClaimsPrincipal user)
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

        if (!scopes.Contains(_requiredScope, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.InsufficientScope);
        }
    }

    private async Task EnsurePaymentAccessAsync(
        ActionExecutingContext context,
        CancellationToken cancellationToken)
    {
        var integrationClientId = TryGetIntegrationClientId(context);

        if (integrationClientId is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var merchantId = TryGetGuidFromActionArguments(
            context,
            "MerchantId");

        if (merchantId is null)
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.MerchantIdRequired);
        }

        var amount = TryGetDecimalFromActionArguments(
            context,
            "Amount") ?? 0m;

        var currency = TryGetStringFromActionArguments(
            context,
            "Currency") ?? string.Empty;

        var hasAccess = await _paymentAccessService.CanCreatePaymentAsync(
            integrationClientId.Value,
            merchantId.Value,
            amount,
            currency,
            cancellationToken);

        if (!hasAccess)
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.PaymentTargetNotAllowed);
        }
    }

    private static Guid? TryGetIntegrationClientId(
        ActionExecutingContext context)
    {
        var integrationClientIdClaim = context.HttpContext.User.FindFirst(
            IntegrationAuthClaimNames.IntegrationClientId)?.Value;

        if (!Guid.TryParse(integrationClientIdClaim, out var integrationClientId))
        {
            return null;
        }

        return integrationClientId;
    }

    private static Guid? TryGetGuidFromActionArguments(
        ActionExecutingContext context,
        string propertyName)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            if (argument is Guid directGuid)
            {
                return directGuid;
            }

            var property = GetProperty(argument, propertyName);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(argument);

            if (value is Guid guidValue)
            {
                return guidValue;
            }

            if (value is string stringValue &&
                Guid.TryParse(stringValue, out var parsedGuid))
            {
                return parsedGuid;
            }
        }

        return null;
    }

    private static decimal? TryGetDecimalFromActionArguments(
        ActionExecutingContext context,
        string propertyName)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var property = GetProperty(argument, propertyName);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(argument);

            if (value is decimal decimalValue)
            {
                return decimalValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is double doubleValue)
            {
                return Convert.ToDecimal(doubleValue);
            }

            if (value is string stringValue &&
                decimal.TryParse(stringValue, out var parsedDecimal))
            {
                return parsedDecimal;
            }
        }

        return null;
    }

    private static string? TryGetStringFromActionArguments(
        ActionExecutingContext context,
        string propertyName)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var property = GetProperty(argument, propertyName);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(argument);

            if (value is string stringValue)
            {
                return stringValue;
            }
        }

        return null;
    }

    private static PropertyInfo? GetProperty(
        object argument,
        string propertyName)
    {
        return argument
            .GetType()
            .GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);
    }
}