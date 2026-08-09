using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.PolicyManagement.Abstractions;
using TravelInsurance.Application.PolicyManagement.DTOs;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.PolicyManagement.Services;

public class PolicyManagementService(IPolicyManagementRepository policyRepository, IAuditLogRepository auditLogRepository) : IPolicyManagementService
{
    public async Task<(PolicyResponse? Policy, OperationResponse Result)> GetByIdAsync(Guid policyId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await policyRepository.GetActorAsync(actorUserId, cancellationToken);
        var policy = await policyRepository.GetPolicyAsync(policyId, cancellationToken);
        return actor is null || policy is null || !WorkflowAccess.CanAccess(actor, policy)
            ? (null, Fail("Policy not found."))
            : (Map(policy), Ok("Policy found."));
    }

    public async Task<IReadOnlyCollection<PolicySummaryResponse>> GetAllAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await policyRepository.GetActorAsync(actorUserId, cancellationToken);
        if (actor is null) return Array.Empty<PolicySummaryResponse>();
        var policies = await policyRepository.GetPoliciesAsync(actor.IsSuperAdmin ? null : actor.CompanyId, cancellationToken);
        return policies.Where(p => WorkflowAccess.CanAccess(actor, p))
            .Select(p => new PolicySummaryResponse(p.Id, p.PolicyNumber, p.PlanId, p.QuoteId, p.IssuanceSource, p.Status, p.GrossPremium, p.Currency, p.EffectiveFrom, p.EffectiveTo, p.IssuedAt))
            .ToArray();
    }

    public async Task<OperationResponse> CancelAsync(Guid policyId, CancelPolicyRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await policyRepository.GetActorAsync(actorUserId, cancellationToken);
        var policy = await policyRepository.GetPolicyAsync(policyId, cancellationToken);
        if (actor is null || policy is null || !WorkflowAccess.CanAccess(actor, policy)) return Fail("Policy not found.");
        if (policy.Status is PolicyStatus.Cancelled or PolicyStatus.Expired) return Fail("Policy is not cancellable.");
        policy.Status = PolicyStatus.Cancelled;
        policy.CancelledAt = DateTimeOffset.UtcNow;
        policy.CancelledByUserId = actor.UserId;
        policy.CancellationReason = request.Reason.Trim();
        if (await policyRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Policy update conflicted. Please retry.");
        await AuditAsync(actor.UserId, "POLICY.CANCELLED", policy.Id, cancellationToken);
        return Ok("Policy cancelled.");
    }

    public async Task<OperationResponse> ExpireAsync(Guid policyId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await policyRepository.GetActorAsync(actorUserId, cancellationToken);
        var policy = await policyRepository.GetPolicyAsync(policyId, cancellationToken);
        if (actor is null || policy is null || !WorkflowAccess.CanAccess(actor, policy)) return Fail("Policy not found.");
        if (policy.Status != PolicyStatus.Active || policy.EffectiveTo >= DateOnly.FromDateTime(DateTime.UtcNow)) return Fail("Only an elapsed active policy can be expired.");
        policy.Status = PolicyStatus.Expired;
        if (await policyRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Policy update conflicted. Please retry.");
        await AuditAsync(actor.UserId, "POLICY.EXPIRED", policy.Id, cancellationToken);
        return Ok("Policy expired.");
    }

    private Task AuditAsync(Guid actorId, string action, Guid id, CancellationToken ct) => auditLogRepository.AddAsync(new AuditLog { ActorUserId = actorId, Action = action, EntityType = "policies", EntityId = id }, ct);
    private static PolicyResponse Map(Policy p) => new(p.Id, p.PolicyNumber, p.PlanId, p.QuoteId, p.IssuanceSource, p.Status, p.GrossPremium, p.Currency, p.EffectiveFrom, p.EffectiveTo,
        p.Passengers.Select(x => new PassengerInput { FullName = x.FullName, DateOfBirth = x.DateOfBirth, PassportNumber = x.PassportNumber, Nationality = x.Nationality }).ToArray(),
        p.CoverageSnapshots.Select(s => new CoverageSnapshotDto(s.CoverageId, s.CoverageCode, s.CoverageName, s.Limit, s.Currency, s.Deductible, s.BenefitSnapshots.Select(b => new BenefitSnapshotDto(b.BenefitId, b.BenefitCode, b.BenefitName, b.LimitTypeId, b.Limit, b.Currency, b.Deductible, b.WaitingPeriodDays)).ToArray())).ToArray());
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}
