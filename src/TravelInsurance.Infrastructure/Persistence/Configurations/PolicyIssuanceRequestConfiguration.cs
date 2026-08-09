using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyIssuanceRequestConfiguration : IEntityTypeConfiguration<PolicyIssuanceRequest>
{
    public void Configure(EntityTypeBuilder<PolicyIssuanceRequest> entity)
    {
        entity.ToTable("policy_issuance_requests", table => table.HasCheckConstraint(
            "CK_issuance_request_source_quote",
            "(issuance_source = 'Quote' AND quote_id IS NOT NULL) OR (issuance_source = 'Direct' AND quote_id IS NULL)"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id");
        entity.Property(x => x.QuoteId).HasColumnName("quote_id");
        entity.Property(x => x.IssuanceSource).HasColumnName("issuance_source").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id");
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.GrossPremium).HasColumnName("gross_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        entity.Property(x => x.PolicyDraftJson).HasColumnName("policy_draft_json").HasColumnType("text");
        entity.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.QuoteId).IsUnique().HasFilter("quote_id IS NOT NULL");
        entity.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Policy).WithOne().HasForeignKey<Policy>(x => x.IssuanceRequestId).OnDelete(DeleteBehavior.Restrict);
    }
}
