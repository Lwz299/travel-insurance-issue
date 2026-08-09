using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Reinsurance.Abstractions;

public interface IReinsuranceRepository
{
    Task<List<ReinsuranceAgreement>> GetAgreementsAsync(Guid? companyId, CancellationToken cancellationToken = default);
    Task<ReinsuranceAgreement?> GetAgreementAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ReinsuranceAgreement>> GetAutomaticAgreementsForPlanAsync(Guid companyId, Guid planId, CancellationToken cancellationToken = default);
    Task<bool> HasCessionForPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<List<ReinsuranceCession>> GetCessionsAsync(Guid? policyId, Guid? agreementId, CancellationToken cancellationToken = default);
    Task<ReinsuranceBordereauBatch?> GetBordereauAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ReinsuranceCession>> GetPendingBordereauCessionsAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<bool> HasBordereauForPeriodAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    void AddAgreement(ReinsuranceAgreement agreement);
    void AddCession(ReinsuranceCession cession);
    void AddBordereau(ReinsuranceBordereauBatch batch);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
