using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class ReinsuranceCession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PolicyId { get; set; }
    public Guid ReinsuranceAgreementId { get; set; }
    public Guid ReinsurerCompanyId { get; set; }
    public Guid? BordereauBatchId { get; set; }
    public string AgreementNumberSnapshot { get; set; } = string.Empty;
    public ReinsuranceCessionStatus Status { get; set; } = ReinsuranceCessionStatus.PendingBordereau;
    public decimal GrossPremium { get; set; }
    public decimal CededPremium { get; set; }
    public decimal GrossSumInsured { get; set; }
    public decimal CededSumInsured { get; set; }
    public decimal CededSharePercentage { get; set; }
    public decimal CompanyRetentionPercentage { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Policy? Policy { get; set; }
    public ReinsuranceAgreement? ReinsuranceAgreement { get; set; }
    public Company? ReinsurerCompany { get; set; }
    public ReinsuranceBordereauBatch? BordereauBatch { get; set; }
}
