using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Claims.DTOs;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Claims.Services;

public class ClaimsService(IClaimsRepository claimsRepository, ICountryRepository countryRepository, IAuditLogRepository auditLogRepository, INumberSequenceService numberSequenceService) : IClaimsService
{
    public async Task<(ClaimResponse? Claim, OperationResponse Result)> RegisterAsync(RegisterClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await claimsRepository.GetActorAsync(actorUserId, cancellationToken);
        var policy = await claimsRepository.GetPolicyAsync(request.PolicyId, cancellationToken);
        if (!CanManage(actor, policy)) return (null, Fail("Policy not found."));
        if (policy!.Passengers.All(p => p.Id != request.PolicyPassengerId)) return (null, Fail("The insured person does not belong to this policy."));
        if (request.IncidentDate < policy.EffectiveFrom || request.IncidentDate > policy.EffectiveTo) return (null, Fail("Incident date must be within the policy coverage period."));
        if (!string.Equals(request.Currency, policy.Currency, StringComparison.OrdinalIgnoreCase)) return (null, Fail("Claim currency must match policy currency."));
        if (request.IncidentCountryId.HasValue && !(await countryRepository.GetExistingIdsAsync([request.IncidentCountryId.Value], cancellationToken)).Contains(request.IncidentCountryId.Value)) return (null, Fail("Incident country does not exist."));

        var claim = new Claim
        {
            PolicyId = policy.Id, PolicyPassengerId = request.PolicyPassengerId, ClaimNumber = await numberSequenceService.NextAsync(policy.Plan!.CompanyId, NumberSequenceType.Claim, cancellationToken), ClaimType = request.ClaimType,
            Status = ClaimStatus.Registered, IncidentDate = request.IncidentDate, IncidentCountryId = request.IncidentCountryId,
            IncidentDescription = request.IncidentDescription.Trim(), RequestedAmount = request.RequestedAmount, Currency = policy.Currency.ToUpperInvariant(),
            InternalNotes = request.InternalNotes?.Trim(), CreatedByUserId = actor!.UserId
        };
        claimsRepository.AddClaim(claim);
        if (await claimsRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Claim registration conflicted. Please retry."));
        await AuditAsync(actor.UserId, "CLAIM.REGISTERED", claim.Id, cancellationToken);
        return (Map(claim), Ok("Claim registered."));
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> GetByIdAsync(Guid claimId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        return claim is null ? (null, Fail("Claim not found.")) : (Map(claim), Ok("Claim found."));
    }

    public async Task<IReadOnlyCollection<ClaimSummaryResponse>> GetAllAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await claimsRepository.GetActorAsync(actorUserId, cancellationToken);
        if (!CanManage(actor, null)) return Array.Empty<ClaimSummaryResponse>();
        var claims = await claimsRepository.GetClaimsAsync(actor!.IsSuperAdmin ? null : actor.CompanyId, cancellationToken);
        return claims.Where(c => CanManage(actor, c.Policy)).Select(c => new ClaimSummaryResponse(c.Id, c.ClaimNumber, c.PolicyId, c.PolicyPassengerId, c.ClaimType, c.Status, c.IncidentDate, c.RequestedAmount, c.ApprovedAmount, c.Currency, c.CreatedAt)).ToArray();
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> AddDocumentAsync(Guid claimId, AddClaimDocumentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        claim.Documents.Add(new ClaimDocument { DocumentType = request.DocumentType.Trim(), FileName = request.FileName.Trim(), FileUrl = request.FileUrl.Trim(), UploadedByUserId = actor!.UserId });
        return await SaveAsync(claim, actor.UserId, "CLAIM.DOCUMENT_ADDED", "Claim document added.", cancellationToken);
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> UpdateStatusAsync(Guid claimId, UpdateClaimStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        if (!IsOperationalTransition(claim.Status, request.Status)) return (null, Fail("This claim status transition is not allowed."));
        Transition(claim, request.Status, request.Reason, actor!.UserId);
        return await SaveAsync(claim, actor.UserId, "CLAIM.STATUS_UPDATED", "Claim status updated.", cancellationToken);
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> ApproveAsync(Guid claimId, ApproveClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        if (claim.Status != ClaimStatus.UnderReview) return (null, Fail("Only a claim under review can be approved."));
        if (request.ApprovedAmount > claim.RequestedAmount) return (null, Fail("Approved amount cannot exceed requested amount."));
        claim.ApprovedAmount = request.ApprovedAmount;
        claim.DecisionReason = request.Reason?.Trim();
        claim.InternalNotes = request.InternalNotes?.Trim() ?? claim.InternalNotes;
        claim.DecidedAt = DateTimeOffset.UtcNow;
        claim.DecidedByUserId = actor!.UserId;
        Transition(claim, ClaimStatus.Approved, request.Reason, actor.UserId);
        return await SaveAsync(claim, actor.UserId, "CLAIM.APPROVED", "Claim approved.", cancellationToken);
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> RejectAsync(Guid claimId, RejectClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        if (claim.Status is not (ClaimStatus.UnderReview or ClaimStatus.MissingDocuments)) return (null, Fail("Only a claim under review can be rejected."));
        claim.DecisionReason = request.Reason.Trim();
        claim.InternalNotes = request.InternalNotes?.Trim() ?? claim.InternalNotes;
        claim.DecidedAt = DateTimeOffset.UtcNow;
        claim.DecidedByUserId = actor!.UserId;
        Transition(claim, ClaimStatus.Rejected, request.Reason, actor.UserId);
        return await SaveAsync(claim, actor.UserId, "CLAIM.REJECTED", "Claim rejected.", cancellationToken);
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> RecordPaymentAsync(Guid claimId, RecordClaimPaymentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        if (claim.Status != ClaimStatus.Approved || !claim.ApprovedAmount.HasValue) return (null, Fail("Only an approved claim can receive a payment."));
        if (!string.Equals(request.Currency, claim.Currency, StringComparison.OrdinalIgnoreCase)) return (null, Fail("Payment currency must match claim currency."));
        if (claim.Payments.Sum(p => p.Amount) + request.Amount > claim.ApprovedAmount.Value) return (null, Fail("Payment amount exceeds the approved claim amount."));
        claim.Payments.Add(new ClaimPayment { Amount = request.Amount, Currency = claim.Currency, PaymentMethod = request.PaymentMethod?.Trim(), PaymentReference = request.PaymentReference?.Trim(), PaidAt = request.PaidAt ?? DateTimeOffset.UtcNow, RecordedByUserId = actor!.UserId });
        if (claim.Payments.Sum(p => p.Amount) == claim.ApprovedAmount.Value) Transition(claim, ClaimStatus.Paid, "Approved amount paid in full.", actor.UserId);
        return await SaveAsync(claim, actor.UserId, "CLAIM.PAYMENT_RECORDED", "Claim payment recorded.", cancellationToken);
    }

    public async Task<(ClaimResponse? Claim, OperationResponse Result)> CloseAsync(Guid claimId, CloseClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, claim) = await LoadAsync(claimId, actorUserId, cancellationToken);
        if (claim is null) return (null, Fail("Claim not found."));
        if (claim.Status is not (ClaimStatus.Paid or ClaimStatus.Rejected)) return (null, Fail("Only a paid or rejected claim can be closed."));
        Transition(claim, ClaimStatus.Closed, request.Reason, actor!.UserId);
        return await SaveAsync(claim, actor.UserId, "CLAIM.CLOSED", "Claim closed.", cancellationToken);
    }

    private async Task<(WorkflowActor? Actor, Claim? Claim)> LoadAsync(Guid claimId, Guid actorUserId, CancellationToken ct)
    {
        var actor = await claimsRepository.GetActorAsync(actorUserId, ct);
        var claim = await claimsRepository.GetClaimAsync(claimId, ct);
        return CanManage(actor, claim?.Policy) ? (actor, claim) : (actor, null);
    }

    private static bool CanManage(WorkflowActor? actor, Policy? policy) => actor is { IsSuperAdmin: true } || (actor is { CompanyType: CompanyType.Insurance, CompanyId: not null } && (policy is null || policy.Plan?.CompanyId == actor.CompanyId));
    private static bool IsOperationalTransition(ClaimStatus from, ClaimStatus to) => (from, to) is (ClaimStatus.Registered, ClaimStatus.UnderReview) or (ClaimStatus.UnderReview, ClaimStatus.MissingDocuments) or (ClaimStatus.MissingDocuments, ClaimStatus.UnderReview);
    private static void Transition(Claim claim, ClaimStatus newStatus, string? reason, Guid actorUserId) { claim.StatusHistory.Add(new ClaimStatusHistory { OldStatus = claim.Status, NewStatus = newStatus, Reason = reason?.Trim(), ChangedByUserId = actorUserId }); claim.Status = newStatus; }
    private async Task<(ClaimResponse? Claim, OperationResponse Result)> SaveAsync(Claim claim, Guid actorUserId, string action, string message, CancellationToken ct) { if (await claimsRepository.SaveChangesAsync(ct) != WorkflowSaveResult.Saved) return (null, Fail("Claim update conflicted. Please retry.")); await AuditAsync(actorUserId, action, claim.Id, ct); return (Map(claim), Ok(message)); }
    private Task AuditAsync(Guid actorId, string action, Guid id, CancellationToken ct) => auditLogRepository.AddAsync(new AuditLog { ActorUserId = actorId, Action = action, EntityType = "claims", EntityId = id }, ct);
    private static ClaimResponse Map(Claim c) => new(c.Id, c.ClaimNumber, c.PolicyId, c.PolicyPassengerId, c.ClaimType, c.Status, c.IncidentDate, c.IncidentCountryId, c.IncidentDescription, c.RequestedAmount, c.ApprovedAmount, c.Currency, c.DecisionReason, c.InternalNotes, c.CreatedAt, c.CreatedByUserId, c.DecidedAt, c.DecidedByUserId, c.Documents.Select(d => new ClaimDocumentResponse(d.Id, d.DocumentType, d.FileName, d.FileUrl, d.UploadedAt, d.UploadedByUserId)).ToArray(), c.Payments.Select(p => new ClaimPaymentResponse(p.Id, p.Amount, p.Currency, p.PaymentMethod, p.PaymentReference, p.PaidAt, p.RecordedByUserId)).ToArray(), c.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new ClaimStatusHistoryResponse(h.Id, h.OldStatus, h.NewStatus, h.Reason, h.ChangedAt, h.ChangedByUserId)).ToArray());
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}
