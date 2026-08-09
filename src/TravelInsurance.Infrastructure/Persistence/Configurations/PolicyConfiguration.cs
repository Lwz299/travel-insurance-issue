using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> entity)
    {
        entity.ToTable("policies", table => table.HasCheckConstraint("CK_policies_issuance_source_quote", "(issuance_source = 'Quote' AND quote_id IS NOT NULL) OR (issuance_source = 'Direct' AND quote_id IS NULL)"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyNumber).HasColumnName("policy_number").HasMaxLength(50).IsRequired();
        entity.HasIndex(x => x.PolicyNumber).IsUnique();
        entity.Property(x => x.PlanId).HasColumnName("plan_id");
        entity.Property(x => x.DestinationCountryId).HasColumnName("destination_country_id");
        entity.Property(x => x.QuoteId).HasColumnName("quote_id");
        entity.HasIndex(x => x.QuoteId).IsUnique();
        entity.Property(x => x.IssuanceRequestId).HasColumnName("issuance_request_id");
        entity.HasIndex(x => x.IssuanceRequestId).IsUnique().HasFilter("issuance_request_id IS NOT NULL");
        entity.Property(x => x.IssuanceSource).HasColumnName("issuance_source").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.IssuedByUserId).HasColumnName("issued_by_user_id");
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.GrossPremium).HasColumnName("gross_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        entity.Property(x => x.IssuedAt).HasColumnName("issued_at");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        entity.Property(x => x.CancelledByUserId).HasColumnName("cancelled_by_user_id");
        entity.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(500);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<Country>().WithMany().HasForeignKey(x => x.DestinationCountryId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Passengers).WithOne(x => x.Policy!).HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.CoverageSnapshots).WithOne(x => x.Policy!).HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.ReinsuranceCessions).WithOne(x => x.Policy!).HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.Claims).WithOne(x => x.Policy!).HasForeignKey(x => x.PolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}
