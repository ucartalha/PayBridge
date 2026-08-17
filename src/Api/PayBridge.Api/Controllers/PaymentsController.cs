using Microsoft.AspNetCore.Mvc;
using PayBridge.Api.Authorization;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Application.Payments.CreatePayment;
using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

namespace PayBridge.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentOrchestrator _paymentOrchestrator;

    public PaymentsController(IPaymentOrchestrator paymentOrchestrator)
    {
        _paymentOrchestrator = paymentOrchestrator;
    }

    [HttpPost]
    [IntegrationPaymentAuthorize("payments:create")]
    public async Task<IActionResult> CreatePayment(
        [FromBody] PaymentExecutionRequest command,
        CancellationToken cancellationToken)
    {
        var result = await _paymentOrchestrator.ExecutePaymentAsync(
            command,
            cancellationToken);

        return Ok(result);
    }
}