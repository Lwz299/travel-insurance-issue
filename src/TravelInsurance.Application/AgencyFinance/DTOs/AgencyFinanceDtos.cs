using System.ComponentModel.DataAnnotations;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.AgencyFinance.DTOs;

public class ConfigureFundingProfileRequest
{
    public AgencyFundingMode Mode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
    [Range(0.01, 999999999)] public decimal? MaximumTransactionAmount { get; set; }
}

public class WalletTopUpRequest
{
    [Required, StringLength(3, MinimumLength = 3)] public string Currency { get; set; } = "USD";
    [Range(0.01, 999999999)] public decimal Amount { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}

public class ConfigureCreditAccountRequest
{
    [Required, StringLength(3, MinimumLength = 3)] public string Currency { get; set; } = "USD";
    [Range(0, 999999999)] public decimal CreditLimit { get; set; }
    [Range(1, 28)] public int BillingDay { get; set; } = 1;
    [Range(0, 365)] public int PaymentDueDays { get; set; } = 30;
    [Range(0, 90)] public int GracePeriodDays { get; set; }
    public bool AutoSuspendOnOverdue { get; set; } = true;
    public bool IsSuspended { get; set; }
}

public class CreateSettlementRequest
{
    [Required, StringLength(3, MinimumLength = 3)] public string Currency { get; set; } = "USD";
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    [Range(0, 999999999)] public decimal AgencyCommission { get; set; }
}

public class RecordSettlementPaymentRequest
{
    [Range(0.01, 999999999)] public decimal Amount { get; set; }
}

public record FundingAuthorizationResponse(Guid Id, Guid? PaymentId, AgencyFundingMode FundingMode, PolicyFundingStatus Status, decimal Amount, string Currency, string FundingReference);
public record FundingProfileResponse(Guid Id, AgencyFundingMode Mode, bool IsDefault, bool IsEnabled, decimal? MaximumTransactionAmount);
public record WalletResponse(Guid Id, Guid AgencyCompanyId, string Currency, decimal Balance, decimal ReservedBalance, decimal AvailableBalance, bool IsActive);
public record CreditAccountResponse(Guid Id, Guid AgencyCompanyId, string Currency, decimal CreditLimit, decimal OutstandingAmount, decimal ReservedAmount, decimal AvailableCredit, bool IsSuspended, int BillingDay, int PaymentDueDays, int GracePeriodDays);
public record SettlementResponse(Guid Id, string SettlementNumber, Guid AgencyCompanyId, string Currency, DateOnly PeriodFrom, DateOnly PeriodTo, decimal GrossPremium, decimal AgencyCommission, decimal Refunds, decimal NetAmountDue, decimal PaidAmount, decimal RemainingAmount, DateOnly DueDate, SettlementStatus Status);
