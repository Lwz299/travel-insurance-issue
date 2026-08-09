using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Common.Options;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Seeding;

public class DevelopmentDataSeeder(
    AppDbContext dbContext,
    IPasswordService passwordService,
    IOptions<SeedOptions> seedOptions,
    ILogger<DevelopmentDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!seedOptions.Value.Enabled)
        {
            return;
        }

        var password = seedOptions.Value.DefaultPassword;
        var passwordHash = passwordService.Hash(password);

        await SeedCompaniesAsync(cancellationToken);
        await SeedUsersAsync(passwordHash, cancellationToken);
        await SeedCatalogAsync(cancellationToken);
        await SeedSamplePlanAsync(cancellationToken);

        logger.LogInformation(
            "Development seed data ready. Default password for all test accounts: {Password}",
            password);
        logger.LogInformation(
            "Test accounts: superadmin@altadamun-ins.test | admin@altadamun-ins.test | agency@altadamun-ins.test | customer@altadamun-ins.test | reinsurer@altadamun-ins.test");
    }

    private async Task SeedCompaniesAsync(CancellationToken cancellationToken)
    {
        var companies = new[]
        {
            new Company
            {
                Id = SeedIds.InsuranceCompanyId,
                Type = CompanyType.Insurance,
                Name = "Al Tadamun Insurance",
                Email = "info@altadamun-ins.com",
                Phone = "+9647700000001",
                Website = "https://altadamun-ins.com",
                Address = "Baghdad, Iraq",
                TaxNumber = "IQ-TAX-001",
                RegistrationNumber = "IQ-REG-001",
                IsActive = true
            },
            new Company
            {
                Id = SeedIds.AgencyCompanyId,
                Type = CompanyType.Agency,
                Name = "Baghdad Travel Agency",
                Email = "agency@altadamun-ins.com",
                Phone = "+9647700000002",
                Address = "Baghdad, Iraq",
                IsActive = true
            },
            new Company
            {
                Id = SeedIds.ReinsurerCompanyId,
                Type = CompanyType.Reinsurer,
                Name = "Gulf Reinsurance Partners",
                Email = "reinsurer@altadamun-ins.com",
                Phone = "+9647700000003",
                IsActive = true
            }
        };

        foreach (var company in companies)
        {
            if (!await dbContext.Companies.AnyAsync(c => c.Id == company.Id, cancellationToken))
            {
                dbContext.Companies.Add(company);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(string passwordHash, CancellationToken cancellationToken)
    {
        var users = new[]
        {
            CreateUser(SeedIds.SuperAdminUserId, "Super Admin", "superadmin@altadamun-ins.test", null, passwordHash),
            CreateUser(SeedIds.AdminUserId, "Insurance Admin", "admin@altadamun-ins.test", SeedIds.InsuranceCompanyId, passwordHash),
            CreateUser(SeedIds.AgencyUserId, "Agency User", "agency@altadamun-ins.test", SeedIds.AgencyCompanyId, passwordHash),
            CreateUser(SeedIds.CustomerUserId, "Test Customer", "customer@altadamun-ins.test", null, passwordHash),
            CreateUser(SeedIds.ReinsurerUserId, "Reinsurer User", "reinsurer@altadamun-ins.test", SeedIds.ReinsurerCompanyId, passwordHash)
        };

        var roleAssignments = new Dictionary<Guid, int[]>
        {
            [SeedIds.SuperAdminUserId] = [5],
            [SeedIds.AdminUserId] = [1],
            [SeedIds.AgencyUserId] = [3],
            [SeedIds.CustomerUserId] = [2],
            [SeedIds.ReinsurerUserId] = [4]
        };

        foreach (var user in users)
        {
            if (await dbContext.Users.AnyAsync(u => u.Email == user.Email, cancellationToken))
            {
                continue;
            }

            dbContext.Users.Add(user);

            foreach (var roleId in roleAssignments[user.Id])
            {
                dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.CoverageGroups.AnyAsync(g => g.Id == SeedIds.MedicalGroupId, cancellationToken))
        {
            return;
        }

        var groups = new[]
        {
            new CoverageGroup { Id = SeedIds.MedicalGroupId, Code = "MEDICAL", Name = "Medical", SortOrder = 1 },
            new CoverageGroup { Id = SeedIds.TripGroupId, Code = "TRIP", Name = "Trip", SortOrder = 2 },
            new CoverageGroup { Id = SeedIds.BaggageGroupId, Code = "BAGGAGE", Name = "Baggage", SortOrder = 3 }
        };

        var benefits = new[]
        {
            new Benefit { Id = SeedIds.EmergencyMedicalBenefitId, BenefitTypeId = 1, Code = "EMERG_MEDICAL", Name = "Emergency Medical Expenses", Description = "Emergency treatment abroad" },
            new Benefit { Id = SeedIds.TripCancellationBenefitId, BenefitTypeId = 1, Code = "TRIP_CANCEL", Name = "Trip Cancellation" },
            new Benefit { Id = SeedIds.BaggageLossBenefitId, BenefitTypeId = 1, Code = "BAGGAGE_LOSS", Name = "Baggage Loss" },
            new Benefit { Id = SeedIds.RepatriationBenefitId, BenefitTypeId = 3, Code = "REPATRIATION", Name = "Medical Repatriation", Description = "Repatriation to home country" }
        };

        var coverages = new[]
        {
            new Coverage
            {
                Id = SeedIds.MedicalHospitalCoverageId,
                GroupId = SeedIds.MedicalGroupId,
                RiskTypeId = 1,
                Code = "MED_HOSPITAL",
                Name = "Medical & Hospital",
                Description = "Emergency treatment abroad",
                IsMandatory = true
            },
            new Coverage
            {
                Id = SeedIds.RepatriationCoverageId,
                GroupId = SeedIds.MedicalGroupId,
                RiskTypeId = 1,
                Code = "REPATRIATION",
                Name = "Repatriation",
                Description = "Medical repatriation to home country"
            },
            new Coverage
            {
                Id = SeedIds.TripCancellationCoverageId,
                GroupId = SeedIds.TripGroupId,
                RiskTypeId = 2,
                Code = "TRIP_CANCEL",
                Name = "Trip Cancellation"
            },
            new Coverage
            {
                Id = SeedIds.BaggageLossCoverageId,
                GroupId = SeedIds.BaggageGroupId,
                RiskTypeId = 3,
                Code = "BAGGAGE",
                Name = "Baggage"
            }
        };

        dbContext.CoverageGroups.AddRange(groups);
        dbContext.Benefits.AddRange(benefits);
        dbContext.Coverages.AddRange(coverages);

        dbContext.CoverageBenefits.AddRange(
            new CoverageBenefit { CoverageId = SeedIds.MedicalHospitalCoverageId, BenefitId = SeedIds.EmergencyMedicalBenefitId, LimitTypeId = 1, LimitValue = 100_000, Currency = "USD", DisplayOrder = 1 },
            new CoverageBenefit { CoverageId = SeedIds.RepatriationCoverageId, BenefitId = SeedIds.RepatriationBenefitId, LimitTypeId = 1, LimitValue = 50_000, Currency = "USD", DisplayOrder = 1 },
            new CoverageBenefit { CoverageId = SeedIds.TripCancellationCoverageId, BenefitId = SeedIds.TripCancellationBenefitId, LimitTypeId = 1, LimitValue = 5_000, Currency = "USD", DisplayOrder = 1 },
            new CoverageBenefit { CoverageId = SeedIds.BaggageLossCoverageId, BenefitId = SeedIds.BaggageLossBenefitId, LimitTypeId = 1, LimitValue = 1_500, Currency = "USD", Deductible = 50, DisplayOrder = 1 });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSamplePlanAsync(CancellationToken cancellationToken)
    {
        var planExists = await dbContext.Plans.AnyAsync(p => p.Id == SeedIds.SamplePlanId, cancellationToken);
        if (planExists)
        {
            var assignmentCount = await dbContext.PlanCoverageAssignments
                .CountAsync(a => a.PlanId == SeedIds.SamplePlanId, cancellationToken);

            if (assignmentCount == 0)
            {
                foreach (var assignment in CreateSampleCoverageAssignments())
                {
                    assignment.PlanId = SeedIds.SamplePlanId;
                    dbContext.PlanCoverageAssignments.Add(assignment);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var plan = new InsurancePlan
        {
            Id = SeedIds.SamplePlanId,
            CompanyId = SeedIds.InsuranceCompanyId,
            Name = "Worldwide Gold Plan",
            Description = "Comprehensive single-trip travel insurance for worldwide destinations.",
            CoverageType = CoverageType.SingleTrip,
            DefaultDurationDays = 30,
            MinAge = 1,
            MaxAge = 75,
            MaxTripDurationDays = 90,
            IsActive = true,
            CoverageAssignments = CreateSampleCoverageAssignments(),
            Countries =
            {
                new PlanCountry { CountryId = SeedIds.IraqCountryId, IsExcluded = false },
                new PlanCountry { CountryId = SeedIds.UaeCountryId, IsExcluded = false }
            },
            PricingRules =
            {
                new PricingRule { MinAge = 1, MaxAge = 17, DurationFrom = 1, DurationTo = 7, Price = 15, Currency = "USD" },
                new PricingRule { MinAge = 1, MaxAge = 17, DurationFrom = 8, DurationTo = 30, Price = 35, Currency = "USD" },
                new PricingRule { MinAge = 18, MaxAge = 65, DurationFrom = 1, DurationTo = 7, Price = 25, Currency = "USD" },
                new PricingRule { MinAge = 18, MaxAge = 65, DurationFrom = 8, DurationTo = 30, Price = 55, Currency = "USD" },
                new PricingRule { MinAge = 66, MaxAge = 75, DurationFrom = 1, DurationTo = 30, Price = 85, Currency = "USD" }
            }
        };

        dbContext.Plans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<PlanCoverageAssignment> CreateSampleCoverageAssignments() =>
    [
        new PlanCoverageAssignment
        {
            CoverageId = SeedIds.MedicalHospitalCoverageId,
            CustomLimit = 100_000,
            Currency = "USD",
            DisplayOrder = 1
        },
        new PlanCoverageAssignment
        {
            CoverageId = SeedIds.RepatriationCoverageId,
            CustomLimit = 50_000,
            Currency = "USD",
            DisplayOrder = 2
        },
        new PlanCoverageAssignment
        {
            CoverageId = SeedIds.TripCancellationCoverageId,
            CustomLimit = 5_000,
            Currency = "USD",
            DisplayOrder = 3
        },
        new PlanCoverageAssignment
        {
            CoverageId = SeedIds.BaggageLossCoverageId,
            CustomLimit = 1_500,
            Currency = "USD",
            CustomDeductible = 50,
            DisplayOrder = 4
        }
    ];

    private static User CreateUser(Guid id, string fullName, string email, Guid? companyId, string passwordHash) => new()
    {
        Id = id,
        FullName = fullName,
        Email = email,
        CompanyId = companyId,
        PasswordHash = passwordHash,
        EmailVerified = true,
        Status = UserStatus.Active
    };
}
