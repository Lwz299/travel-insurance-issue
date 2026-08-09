using Microsoft.Extensions.DependencyInjection;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.AgencyFinance.Services;
using TravelInsurance.Application.Authentication.Services;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.Services;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Claims.Services;
using TravelInsurance.Application.Identity.Abstractions;
using TravelInsurance.Application.Identity.Services;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Organization.Services;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.Services;
using TravelInsurance.Application.Premium.Abstractions;
using TravelInsurance.Application.Premium.Services;
using TravelInsurance.Application.Quotes.Abstractions;
using TravelInsurance.Application.Quotes.Services;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.Services;
using TravelInsurance.Application.PolicyIssuance.Abstractions;
using TravelInsurance.Application.PolicyIssuance.Services;
using TravelInsurance.Application.PolicyManagement.Abstractions;
using TravelInsurance.Application.PolicyManagement.Services;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Application.Settings.Services;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.Services;

namespace TravelInsurance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INumberSequenceService, NumberSequenceService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IAgencyFinanceService, AgencyFinanceService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPlanCoverageService, PlanCoverageService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICoverageZoneService, CoverageZoneService>();
        services.AddScoped<IPremiumCalculationService, PremiumCalculationService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentEventPublisher, InProcessPaymentEventPublisher>();
        services.AddScoped<IPolicyIssuanceService, PolicyIssuanceOrchestrator>();
        services.AddScoped<IPaymentSucceededHandler, PaymentIssuanceCompletionHandler>();
        services.AddScoped<IPolicyManagementService, PolicyManagementService>();
        services.AddScoped<IReinsuranceAgreementService, ReinsuranceAgreementService>();
        services.AddScoped<IReinsuranceCessionService, ReinsuranceCessionService>();
        services.AddScoped<IReinsuranceBordereauService, ReinsuranceBordereauService>();

        services.AddScoped<ICatalogLookupService, CatalogLookupService>();
        services.AddScoped<ICoverageGroupService, CoverageGroupService>();
        services.AddScoped<ICoverageCatalogService, CoverageCatalogService>();
        services.AddScoped<IBenefitCatalogService, BenefitCatalogService>();
        services.AddScoped<ICoverageBenefitMappingService, CoverageBenefitMappingService>();

        return services;
    }
}

