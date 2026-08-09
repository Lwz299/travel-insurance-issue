using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PlanCountryConfiguration : IEntityTypeConfiguration<PlanCountry>
{
    public void Configure(EntityTypeBuilder<PlanCountry> entity)
    {
        entity.ToTable("plan_countries");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.CountryId).HasColumnName("country_id").IsRequired();
        entity.Property(x => x.IsExcluded).HasColumnName("is_excluded").HasDefaultValue(false);

        entity.HasOne(x => x.Country)
            .WithMany(c => c.PlanCountries)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.PlanId, x.CountryId }).IsUnique();
    }
}
