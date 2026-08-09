using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.PolicyIssuance.DTOs;

namespace TravelInsurance.Application.PolicyIssuance.Abstractions;

public interface IPolicyIssuanceService
{
    Task<(IssuanceResponse? Issuance, OperationResponse Result)> IssueFromQuoteAsync(IssueFromQuoteRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(IssuanceResponse? Issuance, OperationResponse Result)> IssueDirectAsync(DirectIssueRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(IssuanceResponse? Issuance, OperationResponse Result)> GetStatusAsync(Guid issuanceRequestId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task HandlePaymentSucceededAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
}
