using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ReinsuranceBordereauBatchConfiguration : IEntityTypeConfiguration<ReinsuranceBordereauBatch>
{
    public void Configure(EntityTypeBuilder<ReinsuranceBordereauBatch> entity)
    {
        entity.ToTable("reinsurance_bordereau_batches", table => table.HasCheckConstraint("CK_reinsurance_bordereau_batches_dates", "period_to >= period_from"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReinsuranceAgreementId).HasColumnName("reinsurance_agreement_id").IsRequired();
        entity.Property(x => x.BatchNumber).HasColumnName("batch_number").HasMaxLength(100).IsRequired();
        entity.Property(x => x.PeriodFrom).HasColumnName("period_from");
        entity.Property(x => x.PeriodTo).HasColumnName("period_to");
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(x => x.GeneratedAt).HasColumnName("generated_at");
        entity.Property(x => x.FinalizedAt).HasColumnName("finalized_at");
        entity.HasIndex(x => x.BatchNumber).IsUnique();
        entity.HasIndex(x => new { x.ReinsuranceAgreementId, x.PeriodFrom, x.PeriodTo }).IsUnique();
        entity.HasOne(x => x.ReinsuranceAgreement).WithMany(a => a.Bordereaux).HasForeignKey(x => x.ReinsuranceAgreementId).OnDelete(DeleteBehavior.Restrict);
    }
}
