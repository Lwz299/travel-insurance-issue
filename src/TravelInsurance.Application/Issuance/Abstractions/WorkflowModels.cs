using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Issuance.Abstractions;

public sealed record WorkflowActor(Guid UserId, Guid? CompanyId, CompanyType? CompanyType, bool IsSuperAdmin);

public enum WorkflowSaveResult
{
    Saved = 1,
    Conflict = 2
}

public static class WorkflowAccess
{
    public static bool CanAccess(WorkflowActor actor, Quote quote) =>
        actor.IsSuperAdmin || quote.CreatedByUserId == actor.UserId ||
        (actor.CompanyId.HasValue && (quote.CompanyId == actor.CompanyId || quote.OwnerCompanyId == actor.CompanyId));

    public static bool CanAccess(WorkflowActor actor, Policy policy) =>
        actor.IsSuperAdmin || policy.IssuedByUserId == actor.UserId ||
        (actor.CompanyId.HasValue && (policy.AgencyCompanyId == actor.CompanyId || policy.Plan?.CompanyId == actor.CompanyId));
}
