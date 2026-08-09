using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class InsurancePlanConfiguration : IEntityTypeConfiguration<InsurancePlan>
{
    public void Configure(EntityTypeBuilder<InsurancePlan> entity)
    {
        entity.ToTable("plans");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CompanyId).HasColumnName("company_id").IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.CoverageType).HasColumnName("coverage_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.DefaultDurationDays).HasColumnName("default_duration_days");
        entity.Property(x => x.MinAge).HasColumnName("min_age");
        entity.Property(x => x.MaxAge).HasColumnName("max_age");
        entity.Property(x => x.MaxTripDurationDays).HasColumnName("max_trip_duration_days");
        entity.Property(x => x.AllowsQuotation).HasColumnName("allows_quotation").HasDefaultValue(true);
        entity.Property(x => x.AllowsDirectIssuance).HasColumnName("allows_direct_issuance").HasDefaultValue(true);
        entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.CoverageAssignments).WithOne(a => a.Plan!).HasForeignKey(a => a.PlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.Countries).WithOne(c => c.Plan!).HasForeignKey(c => c.PlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.CoverageZones).WithOne(z => z.Plan!).HasForeignKey(z => z.PlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.ReinsuranceAgreements).WithOne(a => a.Plan!).HasForeignKey(a => a.PlanId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.PricingRules).WithOne(r => r.Plan!).HasForeignKey(r => r.PlanId).OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.CompanyId);
        entity.HasIndex(x => x.IsActive);

        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
