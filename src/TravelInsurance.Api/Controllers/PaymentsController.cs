using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("payments")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "PAYMENT.RECORD")]
    public async Task<ActionResult<CreatePaymentResponse>> Create([FromBody] CreatePaymentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (payment, result) = await paymentService.CreateAsync(request, userId.Value, idempotencyKey ?? string.Empty, cancellationToken);
        return result.Success && payment is not null ? Ok(payment) : result.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? Conflict(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "PAYMENT.READ")]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (payment, result) = await paymentService.GetByIdAsync(id, userId.Value, cancellationToken);
        return result.Success && payment is not null ? Ok(payment) : NotFound(result);
    }

    [HttpGet("{id:guid}/attempts")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "PAYMENT.READ")]
    public async Task<ActionResult<IReadOnlyCollection<PaymentAttemptResponse>>> GetAttempts(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var attempts = await paymentService.GetAttemptsAsync(id, userId.Value, cancellationToken);
        return attempts.Count == 0 ? NotFound() : Ok(attempts);
    }

    [HttpPost("{id:guid}/retry")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "PAYMENT.RECORD")]
    public async Task<ActionResult<CreatePaymentResponse>> Retry(Guid id, [FromBody] RetryPaymentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (payment, result) = await paymentService.RetryAsync(id, request, userId.Value, idempotencyKey ?? string.Empty, cancellationToken);
        return result.Success && payment is not null ? Ok(payment) : BadRequest(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "PAYMENT.RECORD")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await paymentService.CancelAsync(id, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhooks/{providerCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(string providerCode, [FromBody] string payload, [FromHeader(Name = "X-Payment-Signature")] string? signature, CancellationToken cancellationToken)
    {
        var result = await paymentService.HandleWebhookAsync(providerCode, new GatewayWebhookRequest(providerCode, payload, signature), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
