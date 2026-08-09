using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> entity)
    {
        entity.ToTable("claims", table => table.HasCheckConstraint("CK_claims_requested_amount", "requested_amount > 0"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyId).HasColumnName("policy_id").IsRequired();
        entity.Property(x => x.PolicyPassengerId).HasColumnName("policy_passenger_id").IsRequired();
        entity.Property(x => x.ClaimNumber).HasColumnName("claim_number").HasMaxLength(50).IsRequired();
        entity.Property(x => x.ClaimType).HasColumnName("claim_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.IncidentDate).HasColumnName("incident_date");
        entity.Property(x => x.IncidentCountryId).HasColumnName("incident_country_id");
        entity.Property(x => x.IncidentDescription).HasColumnName("incident_description").HasMaxLength(4000).IsRequired();
        entity.Property(x => x.RequestedAmount).HasColumnName("requested_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.ApprovedAmount).HasColumnName("approved_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
        entity.Property(x => x.InternalNotes).HasColumnName("internal_notes").HasMaxLength(4000);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
        entity.Property(x => x.DecidedByUserId).HasColumnName("decided_by_user_id");
        entity.HasIndex(x => x.ClaimNumber).IsUnique();
        entity.HasIndex(x => new { x.PolicyId, x.Status });
        entity.HasOne(x => x.PolicyPassenger).WithMany().HasForeignKey(x => x.PolicyPassengerId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.IncidentCountry).WithMany().HasForeignKey(x => x.IncidentCountryId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Documents).WithOne(d => d.Claim!).HasForeignKey(d => d.ClaimId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.Payments).WithOne(p => p.Claim!).HasForeignKey(p => p.ClaimId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(x => x.StatusHistory).WithOne(h => h.Claim!).HasForeignKey(h => h.ClaimId).OnDelete(DeleteBehavior.Cascade);
    }
}
