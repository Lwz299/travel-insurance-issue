using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PlanReinsuranceAgreementConfiguration : IEntityTypeConfiguration<PlanReinsuranceAgreement>
{
    public void Configure(EntityTypeBuilder<PlanReinsuranceAgreement> entity)
    {
        entity.ToTable("plan_reinsurance_agreements");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PlanId).HasColumnName("plan_id").IsRequired();
        entity.Property(x => x.ReinsuranceAgreementId).HasColumnName("reinsurance_agreement_id").IsRequired();
        entity.HasOne(x => x.ReinsuranceAgreement).WithMany(a => a.PlanAgreements).HasForeignKey(x => x.ReinsuranceAgreementId).OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(x => new { x.PlanId, x.ReinsuranceAgreementId }).IsUnique();
    }
}
