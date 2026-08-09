using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> entity)
    {
        entity.ToTable("quotes");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.QuoteNumber).HasColumnName("quote_number").HasMaxLength(50).IsRequired();
        entity.HasIndex(x => x.QuoteNumber).IsUnique();
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.CompanyId).HasColumnName("company_id").IsRequired();
        entity.Property(x => x.OwnerCompanyId).HasColumnName("owner_company_id");
        entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        entity.Property(x => x.DestinationCountryId).HasColumnName("destination_country_id");
        entity.Property(x => x.TravelStart).HasColumnName("travel_start");
        entity.Property(x => x.TravelEnd).HasColumnName("travel_end");
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.GrossPremium).HasColumnName("gross_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.CalculatedAt).HasColumnName("calculated_at");
        entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        entity.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
        entity.Property(x => x.SentAt).HasColumnName("sent_at");
        entity.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        entity.Property(x => x.CancelledByUserId).HasColumnName("cancelled_by_user_id");
        entity.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Passengers).WithOne(x => x.Quote!).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.CoverageSnapshots).WithOne(x => x.Quote!).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(x => new { x.PlanId, x.Status });
        entity.HasIndex(x => x.OwnerCompanyId);
    }
}
