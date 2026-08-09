using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> entity)
    {
        entity.ToTable("number_sequences"); entity.HasKey(x => x.Id);
        entity.Property(x => x.CompanyId).HasColumnName("company_id");
        entity.Property(x => x.SequenceType).HasColumnName("sequence_type").HasConversion<string>().HasMaxLength(30);
        entity.Property(x => x.Prefix).HasColumnName("prefix").HasMaxLength(20);
        entity.Property(x => x.CurrentValue).HasColumnName("current_value");
        entity.Property(x => x.IncludeYear).HasColumnName("include_year").HasDefaultValue(true);
        entity.Property(x => x.ResetFrequency).HasColumnName("reset_frequency").HasConversion<string>().HasMaxLength(20);
        entity.Property(x => x.LastResetYear).HasColumnName("last_reset_year");
        entity.HasIndex(x => new { x.CompanyId, x.SequenceType }).IsUnique();
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
