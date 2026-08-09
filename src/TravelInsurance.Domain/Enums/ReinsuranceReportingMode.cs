namespace TravelInsurance.Domain.Enums;

public enum ReinsuranceReportingMode
{
    PeriodicBordereau = 1,
    ImmediateAfterIssue = 2,
    PreApprovalBeforeIssue = 3,
    ImmediateForExceptionsOnly = 4,
    ManualReporting = 5
}
