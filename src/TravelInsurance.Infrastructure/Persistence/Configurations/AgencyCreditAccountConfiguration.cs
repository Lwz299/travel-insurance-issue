using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class AgencyCreditAccountConfiguration : IEntityTypeConfiguration<AgencyCreditAccount>
{
    public void Configure(EntityTypeBuilder<AgencyCreditAccount> entity)
    {
        entity.ToTable("agency_credit_accounts", table => table.HasCheckConstraint("CK_agency_credit_accounts_amounts", "credit_limit >= 0 AND outstanding_amount >= 0 AND reserved_amount >= 0"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AgencyCompanyId).HasColumnName("agency_company_id");
        entity.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        entity.Property(x => x.CreditLimit).HasColumnName("credit_limit").HasColumnType("numeric(14,2)");
        entity.Property(x => x.OutstandingAmount).HasColumnName("outstanding_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.ReservedAmount).HasColumnName("reserved_amount").HasColumnType("numeric(14,2)");
        entity.Property(x => x.BillingDay).HasColumnName("billing_day");
        entity.Property(x => x.PaymentDueDays).HasColumnName("payment_due_days");
        entity.Property(x => x.GracePeriodDays).HasColumnName("grace_period_days");
        entity.Property(x => x.AutoSuspendOnOverdue).HasColumnName("auto_suspend_on_overdue");
        entity.Property(x => x.IsSuspended).HasColumnName("is_suspended");
        entity.HasIndex(x => new { x.AgencyCompanyId, x.Currency }).IsUnique();
        entity.HasOne<Company>().WithMany().HasForeignKey(x => x.AgencyCompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Transactions).WithOne(x => x.CreditAccount!).HasForeignKey(x => x.CreditAccountId).OnDelete(DeleteBehavior.Cascade);
    }
}
