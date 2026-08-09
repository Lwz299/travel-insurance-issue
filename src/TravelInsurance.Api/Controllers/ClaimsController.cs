using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Claims.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("claims")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "CLAIMS.MANAGE")]
public class ClaimsController(IClaimsService claimsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClaimSummaryResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await claimsService.GetAllAsync(User.GetUserId()!.Value, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClaimResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Result(await claimsService.GetByIdAsync(id, User.GetUserId()!.Value, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> Register(RegisterClaimRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.RegisterAsync(request, User.GetUserId()!.Value, cancellationToken), created: true);

    [HttpPost("{id:guid}/documents")]
    public async Task<ActionResult<ClaimResponse>> AddDocument(Guid id, AddClaimDocumentRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.AddDocumentAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ClaimResponse>> UpdateStatus(Guid id, UpdateClaimStatusRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.UpdateStatusAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ClaimResponse>> Approve(Guid id, ApproveClaimRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.ApproveAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ClaimResponse>> Reject(Guid id, RejectClaimRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.RejectAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<ClaimResponse>> RecordPayment(Guid id, RecordClaimPaymentRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.RecordPaymentAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<ClaimResponse>> Close(Guid id, CloseClaimRequest request, CancellationToken cancellationToken) =>
        Result(await claimsService.CloseAsync(id, request, User.GetUserId()!.Value, cancellationToken));

    private ActionResult<ClaimResponse> Result((ClaimResponse? Claim, Application.Authentication.DTOs.OperationResponse Result) operation, bool created = false)
    {
        if (!operation.Result.Success || operation.Claim is null)
            return operation.Result.Message == "Claim not found." ? NotFound(operation.Result) : BadRequest(operation.Result);
        return created ? CreatedAtAction(nameof(GetById), new { id = operation.Claim.Id }, operation.Claim) : Ok(operation.Claim);
    }
}
