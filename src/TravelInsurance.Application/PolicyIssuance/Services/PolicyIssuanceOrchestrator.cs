using System.Text.Json;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.PolicyIssuance.Abstractions;
using TravelInsurance.Application.PolicyIssuance.DTOs;
using TravelInsurance.Application.Premium.Abstractions;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.PolicyIssuance.Services;

/// <summary>
/// Coordinates issuance only. It delegates pricing and payment and creates a policy only when the request is ready.
/// </summary>
public class PolicyIssuanceOrchestrator(
    IInsuranceWorkflowRepository workflowRepository,
    IPremiumCalculationService premiumCalculationService,
    IAgencyFinanceService agencyFinanceService,
    IReinsuranceCessionService reinsuranceCessionService,
    INumberSequenceService numberSequenceService,
    IAuditLogRepository auditLogRepository) : IPolicyIssuanceService
{
    public async Task<(IssuanceResponse? Issuance, OperationResponse Result)> IssueFromQuoteAsync(IssueFromQuoteRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return (null, Fail("Idempotency-Key header is required."));
        var previous = await workflowRepository.GetIssuanceRequestByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (previous is not null) return (Map(previous), Ok("Existing issuance request returned."));

        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        var quote = await workflowRepository.GetQuoteForIssuanceAsync(request.QuoteId, cancellationToken);
        if (actor is null || quote is null || !WorkflowAccess.CanAccess(actor, quote)) return (null, Fail("Quote not found."));
        if (quote.ExpiresAt <= DateTimeOffset.UtcNow || quote.Status != QuoteStatus.Paid) return (null, Fail("Quote must be paid and valid before policy issuance."));
        if (!await workflowRepository.HasPaidQuotePaymentAsync(quote.Id, quote.GrossPremium, quote.Currency, cancellationToken)) return (null, Fail("No matching confirmed payment was found for this quote."));
        if (await workflowRepository.HasPolicyForQuoteAsync(quote.Id, cancellationToken) || await workflowRepository.GetIssuanceRequestByQuoteIdAsync(quote.Id, cancellationToken) is not null) return (null, Fail("This quote already has an issuance request."));

        var issuance = new PolicyIssuanceRequest
        {
            PlanId = quote.PlanId, QuoteId = quote.Id, IssuanceSource = IssuanceSource.Quote, RequestedByUserId = actor.UserId,
            AgencyCompanyId = quote.OwnerCompanyId, IdempotencyKey = idempotencyKey, Status = IssuanceStatus.Processing,
            GrossPremium = quote.GrossPremium, Currency = quote.Currency, EffectiveFrom = quote.TravelStart, EffectiveTo = quote.TravelEnd
        };
        workflowRepository.AddIssuanceRequest(issuance);
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Issuance request conflicted. Please retry."));
        await ProcessAsync(issuance, quote, cancellationToken);
        return (Map(issuance), issuance.Status == IssuanceStatus.Issued ? Ok("Policy issued from quote.") : Fail(issuance.FailureReason ?? "Policy issuance failed."));
    }

    public async Task<(IssuanceResponse? Issuance, OperationResponse Result)> IssueDirectAsync(DirectIssueRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return (null, Fail("Idempotency-Key header is required."));
        var previous = await workflowRepository.GetIssuanceRequestByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (previous is not null) return (Map(previous), Ok("Existing issuance request returned."));

        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        if (actor is null) return (null, Fail("Active user account is required."));
        if (!actor.IsSuperAdmin && request.AgencyCompanyId.HasValue && request.AgencyCompanyId != actor.CompanyId) return (null, Fail("Agency company must match the signed-in user's company."));
        var agencyCompanyId = actor.IsSuperAdmin ? request.AgencyCompanyId : actor.CompanyType == CompanyType.Agency ? actor.CompanyId : null;
        var plan = await workflowRepository.GetPlanForIssuanceAsync(request.PlanId, cancellationToken);
        if (plan is null || !plan.IsActive || !plan.AllowsDirectIssuance) return (null, Fail("This plan does not allow direct issuance."));
        var premium = await premiumCalculationService.CalculateAsync(request, cancellationToken);
        if (!premium.Success) return (null, Fail(premium.Message));

        var issuance = new PolicyIssuanceRequest
        {
            PlanId = plan.Id, IssuanceSource = IssuanceSource.Direct, RequestedByUserId = actor.UserId, AgencyCompanyId = agencyCompanyId,
            IdempotencyKey = idempotencyKey, Status = IssuanceStatus.PendingPayment, GrossPremium = premium.GrossPremium, Currency = premium.Currency,
            EffectiveFrom = request.TravelStart, EffectiveTo = request.TravelEnd,
            PolicyDraftJson = JsonSerializer.Serialize(new DirectIssuancePolicyDraft(request.DestinationCountryId, request.Passengers, premium.CoverageSnapshots))
        };
        workflowRepository.AddIssuanceRequest(issuance);
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Issuance request conflicted. Please retry."));
        await AuditAsync(actor.UserId, "POLICY_ISSUANCE.DIRECT_REQUESTED", issuance.Id, cancellationToken);

        var (funding, fundingResult) = await agencyFinanceService.AuthorizeForIssuanceAsync(issuance, request.Settlement, actor.UserId, idempotencyKey, cancellationToken);
        if (!fundingResult.Success || funding is null) return (Map(issuance), Fail($"Issuance request was created but funding could not be authorized: {fundingResult.Message}"));
        if (funding.Status == PolicyFundingStatus.Reserved)
            await ProcessAsync(issuance, null, cancellationToken);
        var current = await workflowRepository.GetIssuanceRequestReadOnlyAsync(issuance.Id, cancellationToken) ?? issuance;
        return (Map(current, funding.PaymentId), Ok(current.Status == IssuanceStatus.Issued ? "Policy issued." : "Issuance request created and electronic payment started."));
    }

    public async Task<(IssuanceResponse? Issuance, OperationResponse Result)> GetStatusAsync(Guid issuanceRequestId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        var issuance = await workflowRepository.GetIssuanceRequestAsync(issuanceRequestId, cancellationToken);
        if (actor is null || issuance is null || !CanAccess(actor, issuance)) return (null, Fail("Issuance request not found."));
        return (Map(issuance), Ok("Issuance request found."));
    }

    public async Task HandlePaymentSucceededAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default)
    {
        var issuance = await workflowRepository.GetIssuanceRequestAsync(issuanceRequestId, cancellationToken);
        if (issuance is null || issuance.Status is IssuanceStatus.Issued or IssuanceStatus.Cancelled or IssuanceStatus.Rejected) return;
        await ProcessAsync(issuance, null, cancellationToken);
    }

    private async Task ProcessAsync(PolicyIssuanceRequest issuance, Quote? loadedQuote, CancellationToken ct)
    {
        if (issuance.Status == IssuanceStatus.Issued) return;
        issuance.Status = IssuanceStatus.Processing;
        issuance.UpdatedAt = DateTimeOffset.UtcNow;
        Policy? policy;
        var planForNumbering = await workflowRepository.GetPlanForIssuanceAsync(issuance.PlanId, ct);
        if (planForNumbering is null) { await FailIssuanceAsync(issuance, "Plan is unavailable.", ct); return; }
        var policyNumber = await numberSequenceService.NextAsync(planForNumbering.CompanyId, NumberSequenceType.Policy, ct);
        if (issuance.IssuanceSource == IssuanceSource.Quote)
        {
            var quote = loadedQuote ?? await workflowRepository.GetQuoteForIssuanceAsync(issuance.QuoteId!.Value, ct);
            if (quote is null || quote.Status != QuoteStatus.Paid || quote.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                await FailIssuanceAsync(issuance, "Quote is no longer eligible for issuance.", ct);
                return;
            }
            policy = CreateFromQuote(issuance, quote, policyNumber);
            quote.Status = QuoteStatus.ConvertedToPolicy;
            quote.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var draft = string.IsNullOrWhiteSpace(issuance.PolicyDraftJson) ? null : JsonSerializer.Deserialize<DirectIssuancePolicyDraft>(issuance.PolicyDraftJson);
            if (draft is null)
            {
                await FailIssuanceAsync(issuance, "The direct issuance snapshot is unavailable.", ct);
                return;
            }
            policy = CreateFromDirectDraft(issuance, draft, policyNumber);
        }

        workflowRepository.AddPolicy(policy);
        var plan = await workflowRepository.GetPlanForIssuanceAsync(policy.PlanId, ct);
        if (plan is not null)
        {
            await reinsuranceCessionService.CreateForIssuedPolicyAsync(policy, plan.CompanyId, ct);
        }
        issuance.Status = IssuanceStatus.Issued;
        issuance.CompletedAt = DateTimeOffset.UtcNow;
        issuance.UpdatedAt = DateTimeOffset.UtcNow;
        if (await workflowRepository.SaveChangesAsync(ct) != WorkflowSaveResult.Saved)
        {
            issuance.Status = IssuanceStatus.Failed;
            issuance.FailureReason = "Policy issuance conflicted. Please retry.";
            await agencyFinanceService.ReleaseForFailedIssuanceAsync(issuance.Id, ct);
            return;
        }
        if (issuance.IssuanceSource == IssuanceSource.Direct)
        {
            var capture = await agencyFinanceService.CaptureForIssuedPolicyAsync(issuance.Id, policy.Id, ct);
            if (!capture.Success)
                throw new InvalidOperationException($"Policy was issued but funding capture failed: {capture.Message}");
        }
        await AuditAsync(issuance.RequestedByUserId, "POLICY_ISSUANCE.ISSUED", issuance.Id, ct);
    }

    private async Task FailIssuanceAsync(PolicyIssuanceRequest issuance, string reason, CancellationToken ct)
    {
        issuance.Status = IssuanceStatus.Failed;
        issuance.FailureReason = reason;
        issuance.UpdatedAt = DateTimeOffset.UtcNow;
        await workflowRepository.SaveChangesAsync(ct);
        await agencyFinanceService.ReleaseForFailedIssuanceAsync(issuance.Id, ct);
    }

    private static Policy CreateFromQuote(PolicyIssuanceRequest request, Quote quote, string policyNumber) => new()
    {
        PolicyNumber = policyNumber, PlanId = quote.PlanId, DestinationCountryId = quote.DestinationCountryId, QuoteId = quote.Id, IssuanceRequestId = request.Id, IssuanceSource = IssuanceSource.Quote,
        IssuedByUserId = request.RequestedByUserId, AgencyCompanyId = request.AgencyCompanyId, IdempotencyKey = $"issuance:{request.Id}", Status = PolicyStatus.Active,
        GrossPremium = quote.GrossPremium, Currency = quote.Currency, EffectiveFrom = quote.TravelStart, EffectiveTo = quote.TravelEnd,
        Passengers = quote.Passengers.Select(ToPassenger).ToList(), CoverageSnapshots = quote.CoverageSnapshots.Select(ToSnapshot).ToList()
    };

    private static Policy CreateFromDirectDraft(PolicyIssuanceRequest request, DirectIssuancePolicyDraft draft, string policyNumber) => new()
    {
        PolicyNumber = policyNumber, PlanId = request.PlanId, DestinationCountryId = draft.DestinationCountryId, IssuanceRequestId = request.Id, IssuanceSource = IssuanceSource.Direct,
        IssuedByUserId = request.RequestedByUserId, AgencyCompanyId = request.AgencyCompanyId, IdempotencyKey = $"issuance:{request.Id}", Status = PolicyStatus.Active,
        GrossPremium = request.GrossPremium, Currency = request.Currency, EffectiveFrom = request.EffectiveFrom, EffectiveTo = request.EffectiveTo,
        Passengers = draft.Passengers.Select(ToPassenger).ToList(), CoverageSnapshots = draft.CoverageSnapshots.Select(ToSnapshot).ToList()
    };

    private static bool CanAccess(WorkflowActor actor, PolicyIssuanceRequest request) => actor.IsSuperAdmin || request.RequestedByUserId == actor.UserId || (actor.CompanyId.HasValue && request.AgencyCompanyId == actor.CompanyId);
    private static PolicyPassenger ToPassenger(QuotePassenger p) => new() { FullName = p.FullName, DateOfBirth = p.DateOfBirth, PassportNumber = p.PassportNumber, Nationality = p.Nationality };
    private static PolicyPassenger ToPassenger(PassengerInput p) => new() { FullName = p.FullName.Trim(), DateOfBirth = p.DateOfBirth, PassportNumber = p.PassportNumber?.Trim(), Nationality = p.Nationality?.Trim() };
    private static PolicyCoverageSnapshot ToSnapshot(QuoteCoverageSnapshot s) => new() { CoverageId = s.CoverageId, CoverageCode = s.CoverageCode, CoverageName = s.CoverageName, Limit = s.Limit, Currency = s.Currency, Deductible = s.Deductible, BenefitSnapshots = s.BenefitSnapshots.Select(ToSnapshot).ToList() };
    private static PolicyCoverageSnapshot ToSnapshot(CoverageSnapshotDto s) => new() { CoverageId = s.CoverageId, CoverageCode = s.CoverageCode, CoverageName = s.CoverageName, Limit = s.Limit, Currency = s.Currency, Deductible = s.Deductible, BenefitSnapshots = s.Benefits.Select(b => new PolicyBenefitSnapshot { BenefitId = b.BenefitId, BenefitCode = b.BenefitCode, BenefitName = b.BenefitName, LimitTypeId = b.LimitTypeId, Limit = b.Limit, Currency = b.Currency, Deductible = b.Deductible, WaitingPeriodDays = b.WaitingPeriodDays }).ToList() };
    private static PolicyBenefitSnapshot ToSnapshot(QuoteBenefitSnapshot b) => new() { BenefitId = b.BenefitId, BenefitCode = b.BenefitCode, BenefitName = b.BenefitName, LimitTypeId = b.LimitTypeId, Limit = b.Limit, Currency = b.Currency, Deductible = b.Deductible, WaitingPeriodDays = b.WaitingPeriodDays };
    private static IssuanceResponse Map(PolicyIssuanceRequest r, Guid? paymentId = null) => new(r.Id, r.PlanId, r.QuoteId, r.Policy?.Id, paymentId, r.IssuanceSource, r.Status, r.GrossPremium, r.Currency, r.EffectiveFrom, r.EffectiveTo, r.FailureReason);
    private Task AuditAsync(Guid actorId, string action, Guid id, CancellationToken ct) => auditLogRepository.AddAsync(new AuditLog { ActorUserId = actorId, Action = action, EntityType = "policy_issuance_requests", EntityId = id }, ct);
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}



