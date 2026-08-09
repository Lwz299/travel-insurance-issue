using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Reinsurance.Services;

public class ReinsuranceBordereauService(IReinsuranceRepository reinsuranceRepository, INumberSequenceService numberSequenceService) : IReinsuranceBordereauService
{
    public async Task<ReinsuranceBordereauResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var batch = await reinsuranceRepository.GetBordereauAsync(id, cancellationToken);
        return batch is null ? null : Map(batch);
    }

    public async Task<(ReinsuranceBordereauResponse? Bordereau, OperationResponse Result)> GenerateAsync(GenerateBordereauRequest request, CancellationToken cancellationToken = default)
    {
        if (request.PeriodTo < request.PeriodFrom) return (null, Fail("Bordereau end date cannot be before its start date."));
        var agreement = await reinsuranceRepository.GetAgreementAsync(request.ReinsuranceAgreementId, cancellationToken);
        if (agreement is null || !agreement.IsActive) return (null, Fail("Active reinsurance agreement not found."));
        if (agreement.ReportingMode != ReinsuranceReportingMode.PeriodicBordereau) return (null, Fail("This agreement does not use periodic bordereau reporting."));
        if (await reinsuranceRepository.HasBordereauForPeriodAsync(agreement.Id, request.PeriodFrom, request.PeriodTo, cancellationToken)) return (null, Fail("A bordereau already exists for this agreement and period."));
        var cessions = await reinsuranceRepository.GetPendingBordereauCessionsAsync(agreement.Id, request.PeriodFrom, request.PeriodTo, cancellationToken);
        if (cessions.Count == 0) return (null, Fail("No pending cessions exist for this period."));
        var batch = new ReinsuranceBordereauBatch { ReinsuranceAgreementId = agreement.Id, BatchNumber = await numberSequenceService.NextAsync(agreement.CompanyId, NumberSequenceType.Bordereau, cancellationToken), PeriodFrom = request.PeriodFrom, PeriodTo = request.PeriodTo, Status = ReinsuranceBordereauStatus.Generated };
        foreach (var cession in cessions) { cession.BordereauBatchId = batch.Id; cession.Status = ReinsuranceCessionStatus.IncludedInBordereau; }
        batch.Cessions = cessions;
        reinsuranceRepository.AddBordereau(batch);
        await reinsuranceRepository.SaveChangesAsync(cancellationToken);
        return (Map(batch), Ok("Reinsurance bordereau generated."));
    }

    private static ReinsuranceBordereauResponse Map(ReinsuranceBordereauBatch b) => new(b.Id, b.ReinsuranceAgreementId, b.BatchNumber, b.PeriodFrom, b.PeriodTo, b.Status, b.GeneratedAt, b.FinalizedAt, b.Cessions.Select(ReinsuranceCessionService.Map).ToArray());
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}
