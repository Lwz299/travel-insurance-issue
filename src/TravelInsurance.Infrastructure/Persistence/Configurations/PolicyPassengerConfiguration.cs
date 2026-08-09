using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PolicyPassengerConfiguration : IEntityTypeConfiguration<PolicyPassenger>
{
    public void Configure(EntityTypeBuilder<PolicyPassenger> entity)
    {
        entity.ToTable("policy_passengers");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyId).HasColumnName("policy_id");
        entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        entity.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
        entity.Property(x => x.PassportNumber).HasColumnName("passport_number").HasMaxLength(50);
        entity.Property(x => x.Nationality).HasColumnName("nationality").HasMaxLength(100);
    }
}
