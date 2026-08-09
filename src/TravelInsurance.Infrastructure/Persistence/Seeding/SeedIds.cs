namespace TravelInsurance.Infrastructure.Persistence.Seeding;

/// <summary>Fixed IDs for development seed data — use in API tests and .http files.</summary>
public static class SeedIds
{
    public static readonly Guid InsuranceCompanyId = new("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid AgencyCompanyId = new("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid ReinsurerCompanyId = new("a1000000-0000-0000-0000-000000000003");

    public static readonly Guid SuperAdminUserId = new("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid AdminUserId = new("b1000000-0000-0000-0000-000000000002");
    public static readonly Guid AgencyUserId = new("b1000000-0000-0000-0000-000000000003");
    public static readonly Guid CustomerUserId = new("b1000000-0000-0000-0000-000000000004");
    public static readonly Guid ReinsurerUserId = new("b1000000-0000-0000-0000-000000000005");

    public static readonly Guid SamplePlanId = new("d1000000-0000-0000-0000-000000000001");

    public static readonly Guid IraqCountryId = new("c0000000-0000-0000-0000-000000000001");
    public static readonly Guid UaeCountryId = new("c0000000-0000-0000-0000-000000000004");

    // Catalog
    public static readonly Guid MedicalGroupId = new("e1000000-0000-0000-0000-000000000001");
    public static readonly Guid TripGroupId = new("e1000000-0000-0000-0000-000000000002");
    public static readonly Guid BaggageGroupId = new("e1000000-0000-0000-0000-000000000003");

    public static readonly Guid MedicalHospitalCoverageId = new("e2000000-0000-0000-0000-000000000001");
    public static readonly Guid RepatriationCoverageId = new("e2000000-0000-0000-0000-000000000002");
    public static readonly Guid TripCancellationCoverageId = new("e2000000-0000-0000-0000-000000000003");
    public static readonly Guid BaggageLossCoverageId = new("e2000000-0000-0000-0000-000000000004");

    public static readonly Guid EmergencyMedicalBenefitId = new("e3000000-0000-0000-0000-000000000001");
    public static readonly Guid TripCancellationBenefitId = new("e3000000-0000-0000-0000-000000000002");
    public static readonly Guid BaggageLossBenefitId = new("e3000000-0000-0000-0000-000000000003");
    public static readonly Guid RepatriationBenefitId = new("e3000000-0000-0000-0000-000000000004");
}
