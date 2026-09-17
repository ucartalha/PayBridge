using MediatR;
using Microsoft.AspNetCore.Mvc;
using PayBridge.Api.Authorization;
using PayBridge.Api.Errors;
using PayBridge.Api.Extensions;
using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

namespace PayBridge.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IErrorCatalog _errorCatalog;

    public PaymentsController(ISender sender, IErrorCatalog errorCatalog)
    {
        _sender = sender;
        _errorCatalog = errorCatalog;
    }

    [HttpPost]
    [IntegrationPaymentAuthorize("payments:create")]
    public async Task<IActionResult> CreatePayment(
    [FromBody] PaymentExecutionRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            request,
            cancellationToken);

        return result.ToActionResult(
            this,
            _errorCatalog);
    }
}