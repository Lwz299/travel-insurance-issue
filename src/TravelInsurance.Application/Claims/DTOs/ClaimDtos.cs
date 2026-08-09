using System.ComponentModel.DataAnnotations;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Claims.DTOs;

public class RegisterClaimRequest
{
    [Required] public Guid PolicyId { get; set; }
    [Required] public Guid PolicyPassengerId { get; set; }
    public ClaimType ClaimType { get; set; }
    public DateOnly IncidentDate { get; set; }
    public Guid? IncidentCountryId { get; set; }
    [Required, MaxLength(4000)] public string IncidentDescription { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal RequestedAmount { get; set; }
    [Required, MinLength(3), MaxLength(3)] public string Currency { get; set; } = "USD";
    [MaxLength(4000)] public string? InternalNotes { get; set; }
}

public class UpdateClaimStatusRequest
{
    public ClaimStatus Status { get; set; }
    [MaxLength(2000)] public string? Reason { get; set; }
}

public class ApproveClaimRequest
{
    [Range(0.01, double.MaxValue)] public decimal ApprovedAmount { get; set; }
    [MaxLength(2000)] public string? Reason { get; set; }
    [MaxLength(4000)] public string? InternalNotes { get; set; }
}

public class RejectClaimRequest
{
    [Required, MaxLength(2000)] public string Reason { get; set; } = string.Empty;
    [MaxLength(4000)] public string? InternalNotes { get; set; }
}

public class AddClaimDocumentRequest
{
    [Required, MaxLength(100)] public string DocumentType { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string FileName { get; set; } = string.Empty;
    [Required, MaxLength(2000), Url] public string FileUrl { get; set; } = string.Empty;
}

public class RecordClaimPaymentRequest
{
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [Required, MinLength(3), MaxLength(3)] public string Currency { get; set; } = "USD";
    [MaxLength(50)] public string? PaymentMethod { get; set; }
    [MaxLength(100)] public string? PaymentReference { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}

public class CloseClaimRequest
{
    [MaxLength(2000)] public string? Reason { get; set; }
}

public record ClaimDocumentResponse(Guid Id, string DocumentType, string FileName, string FileUrl, DateTimeOffset UploadedAt, Guid UploadedByUserId);
public record ClaimPaymentResponse(Guid Id, decimal Amount, string Currency, string? PaymentMethod, string? PaymentReference, DateTimeOffset PaidAt, Guid RecordedByUserId);
public record ClaimStatusHistoryResponse(Guid Id, ClaimStatus OldStatus, ClaimStatus NewStatus, string? Reason, DateTimeOffset ChangedAt, Guid ChangedByUserId);
public record ClaimSummaryResponse(Guid Id, string ClaimNumber, Guid PolicyId, Guid PolicyPassengerId, ClaimType ClaimType, ClaimStatus Status, DateOnly IncidentDate, decimal RequestedAmount, decimal? ApprovedAmount, string Currency, DateTimeOffset CreatedAt);
public record ClaimResponse(Guid Id, string ClaimNumber, Guid PolicyId, Guid PolicyPassengerId, ClaimType ClaimType, ClaimStatus Status, DateOnly IncidentDate, Guid? IncidentCountryId, string IncidentDescription, decimal RequestedAmount, decimal? ApprovedAmount, string Currency, string? DecisionReason, string? InternalNotes, DateTimeOffset CreatedAt, Guid CreatedByUserId, DateTimeOffset? DecidedAt, Guid? DecidedByUserId, IReadOnlyCollection<ClaimDocumentResponse> Documents, IReadOnlyCollection<ClaimPaymentResponse> Payments, IReadOnlyCollection<ClaimStatusHistoryResponse> StatusHistory);
