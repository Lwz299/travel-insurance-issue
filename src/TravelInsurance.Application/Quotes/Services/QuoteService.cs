using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Premium.Abstractions;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Application.Quotes.Abstractions;
using TravelInsurance.Application.Quotes.DTOs;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Quotes.Services;

public class QuoteService(
    IInsuranceWorkflowRepository workflowRepository,
    IPremiumCalculationService premiumCalculationService,
    INumberSequenceService numberSequenceService,
    IAuditLogRepository auditLogRepository) : IQuoteService
{
    public async Task<(QuoteResponse? Quote, OperationResponse Result)> CreateAsync(CreateQuoteRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        if (actor is null) return (null, Fail("Active user account is required."));
        var plan = await workflowRepository.GetPlanForIssuanceAsync(request.PlanId, cancellationToken);
        if (plan is null || !plan.IsActive || !plan.AllowsQuotation) return (null, Fail("This plan does not allow quotations."));

        var premium = await premiumCalculationService.CalculateAsync(request, cancellationToken);
        if (!premium.Success) return (null, Fail(premium.Message));

        var quote = new Quote
        {
            QuoteNumber = await numberSequenceService.NextAsync(plan.CompanyId, NumberSequenceType.Quote, cancellationToken),
            PlanId = plan.Id, CompanyId = plan.CompanyId, OwnerCompanyId = actor.CompanyId, CreatedByUserId = actor.UserId,
            DestinationCountryId = request.DestinationCountryId, TravelStart = request.TravelStart, TravelEnd = request.TravelEnd,
            GrossPremium = premium.GrossPremium, Currency = premium.Currency, Status = QuoteStatus.Calculated,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(request.ValidForHours),
            Passengers = request.Passengers.Select(ToPassenger).ToList(), CoverageSnapshots = premium.CoverageSnapshots.Select(ToSnapshot).ToList()
        };
        workflowRepository.AddQuote(quote);
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Quote creation conflicted. Please retry."));
        await AuditAsync(actor.UserId, "QUOTE.CREATED", quote.Id, cancellationToken);
        return (Map(quote), Ok("Quote created."));
    }

    public async Task<(QuoteResponse? Quote, OperationResponse Result)> GetByIdAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        var quote = await workflowRepository.GetQuoteForIssuanceAsync(id, cancellationToken);
        if (actor is null || quote is null || !WorkflowAccess.CanAccess(actor, quote)) return (null, Fail("Quote not found."));
        await ExpireIfNeededAsync(quote, cancellationToken);
        return (Map(quote), Ok("Quote found."));
    }

    public Task<OperationResponse> SendAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, actorUserId, QuoteStatus.Calculated, QuoteStatus.Sent, "QUOTE.SENT", cancellationToken);

    public async Task<OperationResponse> AcceptAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, quote, error) = await AuthorizedQuoteAsync(id, actorUserId, cancellationToken);
        if (error is not null) return error;
        await ExpireIfNeededAsync(quote!, cancellationToken);
        if (quote!.Status is not (QuoteStatus.Calculated or QuoteStatus.Sent)) return Fail("Only a calculated or sent quote can be accepted.");
        quote.Status = QuoteStatus.Accepted; quote.AcceptedAt = DateTimeOffset.UtcNow; quote.UpdatedAt = DateTimeOffset.UtcNow;
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Quote update conflicted. Please retry.");
        await AuditAsync(actor!.UserId, "QUOTE.ACCEPTED", quote.Id, cancellationToken);
        return Ok("Quote accepted.");
    }

    public async Task<OperationResponse> CancelAsync(Guid id, CancelQuoteRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var (actor, quote, error) = await AuthorizedQuoteAsync(id, actorUserId, cancellationToken);
        if (error is not null) return error;
        if (quote!.Status is QuoteStatus.Paid or QuoteStatus.ConvertedToPolicy) return Fail("Paid or converted quotes cannot be cancelled.");
        if (quote.Status is QuoteStatus.Cancelled or QuoteStatus.Expired) return Fail("Quote is not cancellable.");
        quote.Status = QuoteStatus.Cancelled; quote.CancelledAt = DateTimeOffset.UtcNow; quote.CancelledByUserId = actor!.UserId;
        quote.CancellationReason = request.Reason.Trim(); quote.UpdatedAt = DateTimeOffset.UtcNow;
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Quote update conflicted. Please retry.");
        await AuditAsync(actor.UserId, "QUOTE.CANCELLED", quote.Id, cancellationToken);
        return Ok("Quote cancelled.");
    }

    private async Task<OperationResponse> TransitionAsync(Guid id, Guid actorUserId, QuoteStatus from, QuoteStatus to, string action, CancellationToken cancellationToken)
    {
        var (actor, quote, error) = await AuthorizedQuoteAsync(id, actorUserId, cancellationToken);
        if (error is not null) return error;
        await ExpireIfNeededAsync(quote!, cancellationToken);
        if (quote!.Status != from) return Fail($"Quote must be {from} before this operation.");
        quote.Status = to; quote.SentAt = to == QuoteStatus.Sent ? DateTimeOffset.UtcNow : quote.SentAt; quote.UpdatedAt = DateTimeOffset.UtcNow;
        if (await workflowRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Quote update conflicted. Please retry.");
        await AuditAsync(actor!.UserId, action, quote.Id, cancellationToken);
        return Ok("Quote updated.");
    }

    private async Task<(WorkflowActor? Actor, Quote? Quote, OperationResponse? Error)> AuthorizedQuoteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        var quote = await workflowRepository.GetQuoteForIssuanceAsync(id, cancellationToken);
        return actor is null || quote is null || !WorkflowAccess.CanAccess(actor, quote) ? (actor, quote, Fail("Quote not found.")) : (actor, quote, null);
    }

    private async Task ExpireIfNeededAsync(Quote quote, CancellationToken cancellationToken)
    {
        if (quote.ExpiresAt > DateTimeOffset.UtcNow || quote.Status is QuoteStatus.Paid or QuoteStatus.ConvertedToPolicy or QuoteStatus.Cancelled or QuoteStatus.Expired) return;
        quote.Status = QuoteStatus.Expired; quote.UpdatedAt = DateTimeOffset.UtcNow;
        await workflowRepository.SaveChangesAsync(cancellationToken);
    }

    private static QuotePassenger ToPassenger(PassengerInput input) => new() { FullName = input.FullName.Trim(), DateOfBirth = input.DateOfBirth, PassportNumber = input.PassportNumber?.Trim(), Nationality = input.Nationality?.Trim() };
    private static QuoteCoverageSnapshot ToSnapshot(CoverageSnapshotDto dto) => new()
    {
        CoverageId = dto.CoverageId, CoverageCode = dto.CoverageCode, CoverageName = dto.CoverageName, Limit = dto.Limit, Currency = dto.Currency, Deductible = dto.Deductible,
        BenefitSnapshots = dto.Benefits.Select(b => new QuoteBenefitSnapshot { BenefitId = b.BenefitId, BenefitCode = b.BenefitCode, BenefitName = b.BenefitName, LimitTypeId = b.LimitTypeId, Limit = b.Limit, Currency = b.Currency, Deductible = b.Deductible, WaitingPeriodDays = b.WaitingPeriodDays }).ToList()
    };
    internal static QuoteResponse Map(Quote quote) => new(quote.Id, quote.QuoteNumber, quote.PlanId, quote.CompanyId, quote.Status, quote.TravelStart, quote.TravelEnd, quote.GrossPremium, quote.Currency, quote.ExpiresAt, quote.AcceptedAt,
        quote.Passengers.Select(p => new PassengerInput { FullName = p.FullName, DateOfBirth = p.DateOfBirth, PassportNumber = p.PassportNumber, Nationality = p.Nationality }).ToArray(),
        quote.CoverageSnapshots.Select(s => new CoverageSnapshotDto(s.CoverageId, s.CoverageCode, s.CoverageName, s.Limit, s.Currency, s.Deductible, s.BenefitSnapshots.Select(b => new BenefitSnapshotDto(b.BenefitId, b.BenefitCode, b.BenefitName, b.LimitTypeId, b.Limit, b.Currency, b.Deductible, b.WaitingPeriodDays)).ToArray())).ToArray());
    private Task AuditAsync(Guid actorUserId, string action, Guid entityId, CancellationToken ct) => auditLogRepository.AddAsync(new AuditLog { ActorUserId = actorUserId, Action = action, EntityType = "quotes", EntityId = entityId }, ct);
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}
