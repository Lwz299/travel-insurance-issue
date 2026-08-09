using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permissions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(x => x.Module).HasColumnName("module").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        entity.HasIndex(x => x.Name).IsUnique();

        entity.HasData(
            new Permission { Id = 1, Name = "POLICY.READ", Module = "Policy", Description = "Read policies" },
            new Permission { Id = 2, Name = "POLICY.ISSUE", Module = "Policy", Description = "Issue policies" },
            new Permission { Id = 3, Name = "PAYMENT.READ", Module = "Payment", Description = "Read payments" },
            new Permission { Id = 4, Name = "AUTH.MANAGE_SESSIONS", Module = "Auth", Description = "Manage sessions" },
            new Permission { Id = 5, Name = "PLAN.MANAGE", Module = "Plan", Description = "Manage plans" },
            new Permission { Id = 6, Name = "QUOTE.GENERATE", Module = "Quote", Description = "Generate quotes" },
            new Permission { Id = 7, Name = "COMPANY.MANAGE", Module = "Organization", Description = "Manage companies" },
            new Permission { Id = 8, Name = "AGENCY.MANAGE", Module = "Organization", Description = "Manage agencies" },
            new Permission { Id = 9, Name = "USER.MANAGE", Module = "Identity", Description = "Manage users" },
            new Permission { Id = 10, Name = "REPORT.VIEW", Module = "Report", Description = "View reports" },
            new Permission { Id = 11, Name = "CATALOG.MANAGE", Module = "Catalog", Description = "Manage insurance product catalog" },
            new Permission { Id = 12, Name = "QUOTE.CREATE", Module = "Quote", Description = "Create quotations" },
            new Permission { Id = 13, Name = "QUOTE.ACCEPT", Module = "Quote", Description = "Accept quotations" },
            new Permission { Id = 14, Name = "PAYMENT.RECORD", Module = "Payment", Description = "Record confirmed payments" },
            new Permission { Id = 15, Name = "POLICY.ISSUE_FROM_QUOTE", Module = "Policy", Description = "Issue policies from paid quotes" },
            new Permission { Id = 16, Name = "POLICY.ISSUE_DIRECT", Module = "Policy", Description = "Issue policies directly" },
            new Permission { Id = 17, Name = "QUOTE.SEND", Module = "Quote", Description = "Send quotations" },
            new Permission { Id = 18, Name = "QUOTE.CANCEL", Module = "Quote", Description = "Cancel quotations" },
            new Permission { Id = 19, Name = "PAYMENT.CONFIRM", Module = "Payment", Description = "Confirm payments from trusted payment processing" },
            new Permission { Id = 20, Name = "POLICY.CANCEL", Module = "Policy", Description = "Cancel policies" },
            new Permission { Id = 21, Name = "POLICY.EXPIRE", Module = "Policy", Description = "Expire elapsed policies" },
            new Permission { Id = 22, Name = "CLAIMS.MANAGE", Module = "Claims", Description = "Manage insurance claims" },
            new Permission { Id = 23, Name = "AGENCY.FINANCE.MANAGE", Module = "Agency Finance", Description = "Manage agency wallets, credit and settlements" }
        );
    }
}

