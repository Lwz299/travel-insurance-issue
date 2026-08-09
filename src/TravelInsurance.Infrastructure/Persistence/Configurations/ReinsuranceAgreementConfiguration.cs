using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ReinsuranceAgreementConfiguration : IEntityTypeConfiguration<ReinsuranceAgreement>
{
    public void Configure(EntityTypeBuilder<ReinsuranceAgreement> entity)
    {
        entity.ToTable("reinsurance_agreements", table =>
        {
            table.HasCheckConstraint("CK_reinsurance_agreements_dates", "effective_to >= effective_from");
            table.HasCheckConstraint("CK_reinsurance_agreements_ceded_share", "ceded_share_percentage > 0 AND ceded_share_percentage <= 100");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CompanyId).HasColumnName("company_id").IsRequired();
        entity.Property(x => x.ReinsurerCompanyId).HasColumnName("reinsurer_company_id").IsRequired();
        entity.Property(x => x.AgreementNumber).HasColumnName("agreement_number").HasMaxLength(100).IsRequired();
        entity.Property(x => x.AgreementName).HasColumnName("agreement_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Type).HasColumnName("agreement_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        entity.Property(x => x.ReportingMode).HasColumnName("reporting_mode").HasConversion<string>().HasMaxLength(40).IsRequired();
        entity.Property(x => x.ReportingFrequency).HasColumnName("reporting_frequency").HasConversion<string>().HasMaxLength(20);
        entity.Property(x => x.RequiresPreApproval).HasColumnName("requires_pre_approval").HasDefaultValue(false);
        entity.Property(x => x.Priority).HasColumnName("priority").HasDefaultValue(100);
        entity.Property(x => x.CededSharePercentage).HasColumnName("ceded_share_percentage").HasColumnType("numeric(5,2)");
        entity.Property(x => x.MaxTripDurationDays).HasColumnName("max_trip_duration_days");
        entity.Property(x => x.MaxCoverageLimit).HasColumnName("max_coverage_limit").HasColumnType("numeric(14,2)");
        entity.Property(x => x.MinAge).HasColumnName("min_age");
        entity.Property(x => x.MaxAge).HasColumnName("max_age");
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.HasIndex(x => new { x.CompanyId, x.AgreementNumber }).IsUnique();
        entity.HasIndex(x => new { x.CompanyId, x.IsActive });
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ReinsurerCompany).WithMany().HasForeignKey(x => x.ReinsurerCompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
