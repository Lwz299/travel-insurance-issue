using Microsoft.EntityFrameworkCore;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<InsurancePlan> Plans => Set<InsurancePlan>();
    public DbSet<PlanCountry> PlanCountries => Set<PlanCountry>();
    public DbSet<CoverageZone> CoverageZones => Set<CoverageZone>();
    public DbSet<CoverageZoneCountry> CoverageZoneCountries => Set<CoverageZoneCountry>();
    public DbSet<PlanCoverageZone> PlanCoverageZones => Set<PlanCoverageZone>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<ReinsuranceAgreement> ReinsuranceAgreements => Set<ReinsuranceAgreement>();
    public DbSet<PlanReinsuranceAgreement> PlanReinsuranceAgreements => Set<PlanReinsuranceAgreement>();
    public DbSet<ReinsuranceAgreementCoverageZone> ReinsuranceAgreementCoverageZones => Set<ReinsuranceAgreementCoverageZone>();
    public DbSet<ReinsuranceCession> ReinsuranceCessions => Set<ReinsuranceCession>();
    public DbSet<ReinsuranceBordereauBatch> ReinsuranceBordereauBatches => Set<ReinsuranceBordereauBatch>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimPayment> ClaimPayments => Set<ClaimPayment>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();

    // Catalog
    public DbSet<BenefitType> BenefitTypes => Set<BenefitType>();
    public DbSet<LimitType> LimitTypes => Set<LimitType>();
    public DbSet<RiskType> RiskTypes => Set<RiskType>();
    public DbSet<CoverageGroup> CoverageGroups => Set<CoverageGroup>();
    public DbSet<Coverage> Coverages => Set<Coverage>();
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<CoverageBenefit> CoverageBenefits => Set<CoverageBenefit>();
    public DbSet<PlanCoverageAssignment> PlanCoverageAssignments => Set<PlanCoverageAssignment>();
    public DbSet<PlanCoverageBenefitOverride> PlanCoverageBenefitOverrides => Set<PlanCoverageBenefitOverride>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuotePassenger> QuotePassengers => Set<QuotePassenger>();
    public DbSet<QuoteCoverageSnapshot> QuoteCoverageSnapshots => Set<QuoteCoverageSnapshot>();
    public DbSet<QuoteBenefitSnapshot> QuoteBenefitSnapshots => Set<QuoteBenefitSnapshot>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyIssuanceRequest> PolicyIssuanceRequests => Set<PolicyIssuanceRequest>();
    public DbSet<PolicyPassenger> PolicyPassengers => Set<PolicyPassenger>();
    public DbSet<PolicyCoverageSnapshot> PolicyCoverageSnapshots => Set<PolicyCoverageSnapshot>();
    public DbSet<PolicyBenefitSnapshot> PolicyBenefitSnapshots => Set<PolicyBenefitSnapshot>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<AgencyFundingProfile> AgencyFundingProfiles => Set<AgencyFundingProfile>();
    public DbSet<AgencyWallet> AgencyWallets => Set<AgencyWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<AgencyCreditAccount> AgencyCreditAccounts => Set<AgencyCreditAccount>();
    public DbSet<AgencyCreditTransaction> AgencyCreditTransactions => Set<AgencyCreditTransaction>();
    public DbSet<AgencySettlement> AgencySettlements => Set<AgencySettlement>();
    public DbSet<PolicyFunding> PolicyFundings => Set<PolicyFunding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

