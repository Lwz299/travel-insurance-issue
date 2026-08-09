using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
{
    public void Configure(EntityTypeBuilder<ClaimStatusHistory> entity)
    {
        entity.ToTable("claim_status_history");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClaimId).HasColumnName("claim_id").IsRequired();
        entity.Property(x => x.OldStatus).HasColumnName("old_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.NewStatus).HasColumnName("new_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
        entity.Property(x => x.ChangedAt).HasColumnName("changed_at");
        entity.Property(x => x.ChangedByUserId).HasColumnName("changed_by_user_id");
        entity.HasIndex(x => new { x.ClaimId, x.ChangedAt });
    }
}
