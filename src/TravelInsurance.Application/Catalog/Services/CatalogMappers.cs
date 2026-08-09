using TravelInsurance.Application.Catalog.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Services;

internal static class CatalogMappers
{
    public static CoverageResponse MapCoverage(Coverage c) => new(
        c.Id, c.GroupId, c.Group?.Name ?? string.Empty, c.RiskTypeId, c.RiskType?.Name ?? string.Empty,
        c.Code, c.Name, c.Description, c.IsMandatory, c.IsActive);

    public static CoverageDetailResponse MapCoverageDetail(Coverage c) => new(
        c.Id, c.GroupId, c.Group?.Name ?? string.Empty, c.RiskTypeId, c.RiskType?.Name ?? string.Empty,
        c.Code, c.Name, c.Description, c.IsMandatory, c.IsActive,
        c.CoverageBenefits.OrderBy(x => x.DisplayOrder).Select(MapCoverageBenefit).ToArray());

    public static CoverageBenefitResponse MapCoverageBenefit(CoverageBenefit m) => new(
        m.Id, m.BenefitId, m.Benefit?.Code ?? string.Empty, m.Benefit?.Name ?? string.Empty,
        m.LimitTypeId, m.LimitType?.Name ?? string.Empty,
        m.LimitValue, m.Currency, m.Deductible, m.WaitingPeriodDays, m.DisplayOrder, m.IsOptional);

    public static BenefitResponse MapBenefit(Benefit b) => new(
        b.Id, b.BenefitTypeId, b.BenefitType?.Name ?? string.Empty, b.Code, b.Name, b.Description, b.IsActive);
}
