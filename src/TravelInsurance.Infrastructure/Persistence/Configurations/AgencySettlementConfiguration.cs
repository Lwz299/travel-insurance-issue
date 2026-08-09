using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AgencySettlementConfiguration : IEntityTypeConfiguration<AgencySettlement>
{
    public void Configure(EntityTypeBuilder<AgencySettlement> entity)
    {
        entity.ToTable("agency_settlements");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.SettlementNumber).HasColumnName("settlement_number").HasMaxLength(50).IsRequired();
        entity.Property(x => x.PeriodFrom).HasColumnName("period_from");
        entity.Property(x => x.PeriodTo).HasColumnName("period_to");
        entity.Property(x => x.GrossPremium).HasColumnName("gross_premium").HasColumnType("numeric(14,2)");
        entity.Property(x => x.AgencyCommission).HasColumnName("agency_commission").HasColumnType("numeric(14,2)");
        entity.Property(x => x.Refunds).HasColumnName("refunds").HasColumnType("numeric(14,2)");
        entity.Property(x => x.NetAmountDue).HasColumnName("net_amount_due").HasColumnType("numeric(14,2)");
        entity.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.RemainingAmount).HasColumnName("remaining_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.DueDate).HasColumnName("due_date");
        entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        entity.HasIndex(x => x.SettlementNumber).IsUnique();
        entity.HasIndex(x => new { x.AgencyCompanyId, x.Currency, x.PeriodFrom, x.PeriodTo }).IsUnique();
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.AgencyCompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
