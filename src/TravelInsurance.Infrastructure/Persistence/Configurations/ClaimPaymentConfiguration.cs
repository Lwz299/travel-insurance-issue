using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ClaimPaymentConfiguration : IEntityTypeConfiguration<ClaimPayment>
{
    public void Configure(EntityTypeBuilder<ClaimPayment> entity)
    {
        entity.ToTable("claim_payments", table => table.HasCheckConstraint("CK_claim_payments_amount", "amount > 0"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClaimId).HasColumnName("claim_id").IsRequired();
        entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
        entity.Property(x => x.PaymentReference).HasColumnName("payment_reference").HasMaxLength(100);
        entity.Property(x => x.PaidAt).HasColumnName("paid_at");
        entity.Property(x => x.RecordedByUserId).HasColumnName("recorded_by_user_id");
        entity.HasIndex(x => x.ClaimId);
    }
}
