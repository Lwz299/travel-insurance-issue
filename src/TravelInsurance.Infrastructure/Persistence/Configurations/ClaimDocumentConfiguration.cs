using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> entity)
    {
        entity.ToTable("claim_documents");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClaimId).HasColumnName("claim_id").IsRequired();
        entity.Property(x => x.DocumentType).HasColumnName("document_type").HasMaxLength(100).IsRequired();
        entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        entity.Property(x => x.FileUrl).HasColumnName("file_url").HasMaxLength(2000).IsRequired();
        entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
        entity.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id");
    }
}
