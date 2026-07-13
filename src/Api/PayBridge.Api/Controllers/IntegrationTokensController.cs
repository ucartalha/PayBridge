using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;

namespace PayBridge.Api.Controllers;

[ApiController]
[Route("api/integration-tokens")]
public sealed class IntegrationTokensController : ControllerBase
{
    private readonly IIntegrationClientStore _integrationClientStore;
    private readonly IIntegrationTokenService _integrationTokenService;

    public IntegrationTokensController(
        IIntegrationClientStore integrationClientStore,
        IIntegrationTokenService integrationTokenService)
    {
        _integrationClientStore = integrationClientStore;
        _integrationTokenService = integrationTokenService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateToken(
        [FromBody] IssueIntegrationTokenRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _integrationClientStore.ValidateAsync(
            request.ClientCode,
            request.ClientSecret,
            cancellationToken);

        if (client is null)
        {
            throw new BusinessException(
                (int)IntegrationAuthErrorCode.InvalidClient);
        }

        var token = await _integrationTokenService.IssueAsync(
            client,
            cancellationToken);

        return Ok(token);
    }
}