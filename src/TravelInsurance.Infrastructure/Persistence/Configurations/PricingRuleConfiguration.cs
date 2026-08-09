using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> entity)
    {
        entity.ToTable("pricing_rules");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.MinAge).HasColumnName("min_age");
        entity.Property(x => x.MaxAge).HasColumnName("max_age");
        entity.Property(x => x.DurationFrom).HasColumnName("duration_from");
        entity.Property(x => x.DurationTo).HasColumnName("duration_to");
        entity.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasIndex(x => x.PlanId);
        entity.HasIndex(x => new { x.PlanId, x.MinAge, x.MaxAge, x.DurationFrom, x.DurationTo });
    }
}
