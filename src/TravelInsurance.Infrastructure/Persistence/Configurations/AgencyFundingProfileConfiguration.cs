using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AgencyFundingProfileConfiguration : IEntityTypeConfiguration<AgencyFundingProfile>
{
    public void Configure(EntityTypeBuilder<AgencyFundingProfile> entity)
    {
        entity.ToTable("agency_funding_profiles");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.IsDefault).HasColumnName("is_default");
        entity.Property(x => x.IsEnabled).HasColumnName("is_enabled");
        entity.Property(x => x.MaximumTransactionAmount).HasColumnName("maximum_transaction_amount").HasColumnType("numeric(14,2)");
        entity.HasIndex(x => new { x.AgencyCompanyId, x.Mode }).IsUnique();
        entity.HasIndex(x => new { x.AgencyCompanyId, x.IsDefault }).HasFilter("is_default = true").IsUnique();
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.AgencyCompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
