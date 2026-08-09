using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> entity)
    {
        entity.ToTable("payment_attempts");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PaymentId).HasColumnName("payment_id");
        entity.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(50).IsRequired();
        entity.Property(x => x.ProviderReference).HasColumnName("provider_reference").HasMaxLength(150);
        entity.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.CheckoutUrl).HasColumnName("checkout_url").HasMaxLength(2000);
        entity.Property(x => x.FailureCode).HasColumnName("failure_code").HasMaxLength(100);
        entity.Property(x => x.FailureMessage).HasColumnName("failure_message").HasMaxLength(500);
        entity.Property(x => x.SafeMetadata).HasColumnName("safe_metadata");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
        entity.HasIndex(x => new { x.ProviderCode, x.IdempotencyKey }).IsUnique();
        entity.HasIndex(x => new { x.ProviderCode, x.ProviderReference }).IsUnique().HasFilter("provider_reference IS NOT NULL");
        entity.HasIndex(x => x.PaymentId);
    }
}
