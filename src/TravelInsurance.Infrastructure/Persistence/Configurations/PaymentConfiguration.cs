using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.ToTable("payments", table => table.HasCheckConstraint("CK_payments_single_target", "(quote_id IS NOT NULL AND policy_id IS NULL AND issuance_request_id IS NULL) OR (quote_id IS NULL AND policy_id IS NOT NULL AND issuance_request_id IS NULL) OR (quote_id IS NULL AND policy_id IS NULL AND issuance_request_id IS NOT NULL)"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.QuoteId).HasColumnName("quote_id");
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.IssuanceRequestId).HasColumnName("issuance_request_id");
        entity.Property(x => x.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(50).IsRequired();
        entity.Property(x => x.BusinessReference).HasColumnName("business_reference").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Method).HasColumnName("method").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Purpose).HasColumnName("purpose").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
        entity.HasIndex(x => x.ReferenceNumber).IsUnique();
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.QuoteId);
        entity.HasIndex(x => x.PolicyId);
        entity.HasIndex(x => x.IssuanceRequestId);
        entity.HasOne<PolicyIssuanceRequest>().WithMany().HasForeignKey(x => x.IssuanceRequestId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Attempts).WithOne(x => x.Payment!).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
    }
}
