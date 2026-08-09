using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Quotes.Abstractions;
using TravelInsurance.Application.Quotes.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("quotes")]
[Authorize]
public class QuotesController(IQuoteService quoteService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "QUOTE.CREATE")]
    public async Task<ActionResult<QuoteResponse>> Create([FromBody] CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (quote, result) = await quoteService.CreateAsync(request, userId.Value, cancellationToken);
        return result.Success && quote is not null ? CreatedAtAction(nameof(GetById), new { id = quote.Id }, quote) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuoteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (quote, result) = await quoteService.GetByIdAsync(id, userId.Value, cancellationToken);
        return result.Success && quote is not null ? Ok(quote) : NotFound(result);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "QUOTE.ACCEPT")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await quoteService.AcceptAsync(id, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "QUOTE.SEND")]
    public async Task<IActionResult> Send(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await quoteService.SendAsync(id, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "QUOTE.CANCEL")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelQuoteRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await quoteService.CancelAsync(id, request, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
