using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("audit_logs");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
        entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        entity.Property(x => x.EntityId).HasColumnName("entity_id");
        entity.Property(x => x.Metadata).HasColumnName("metadata");
        entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(80);
        entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.HasIndex(x => x.Action);
        entity.HasIndex(x => x.CreatedAt);
    }
}
