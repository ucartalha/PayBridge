using MediatR;
using Microsoft.AspNetCore.Mvc;
using PayBridge.Api.Authorization;
using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

namespace PayBridge.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
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

        return Ok(result);
    }
}