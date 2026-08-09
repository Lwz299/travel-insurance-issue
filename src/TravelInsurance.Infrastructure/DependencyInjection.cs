using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Common.Options;
using TravelInsurance.Application.Identity.Abstractions;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.PolicyManagement.Abstractions;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Infrastructure.Payments;
using TravelInsurance.Infrastructure.Email;
using TravelInsurance.Infrastructure.Persistence;
using TravelInsurance.Infrastructure.Persistence.Repositories;
using TravelInsurance.Infrastructure.Persistence.Seeding;
using TravelInsurance.Infrastructure.Security;

namespace TravelInsurance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        services.Configure<BrevoOptions>(configuration.GetSection(BrevoOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<DummyPaymentGatewayOptions>(configuration.GetSection(DummyPaymentGatewayOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IInsuranceWorkflowRepository, InsuranceWorkflowRepository>();
        services.AddScoped<IPolicyManagementRepository, PolicyManagementRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReinsuranceRepository, ReinsuranceRepository>();
        services.AddScoped<INumberSequenceRepository, NumberSequenceRepository>();
        services.AddHostedService<QuoteExpirationHostedService>();
        services.AddScoped<IDummyPaymentScenarioResolver, ConfigurationDummyPaymentScenarioResolver>();
        services.AddScoped<IPaymentGateway, DummyPaymentGateway>();
        services.AddScoped<IPaymentGatewayResolver, PaymentGatewayResolver>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IClaimsRepository, ClaimsRepository>();
        services.AddScoped<IAgencyFinanceRepository, AgencyFinanceRepository>();
        services.AddScoped<IPlanCoverageRepository, PlanCoverageRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICoverageZoneRepository, CoverageZoneRepository>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<DevelopmentDataSeeder>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        services.AddHttpClient<IEmailService, BrevoEmailService>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}

