using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Claims.DTOs;

namespace TravelInsurance.Application.Claims.Abstractions;

public interface IClaimsService
{
    Task<(ClaimResponse? Claim, OperationResponse Result)> RegisterAsync(RegisterClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> GetByIdAsync(Guid claimId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClaimSummaryResponse>> GetAllAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> AddDocumentAsync(Guid claimId, AddClaimDocumentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> UpdateStatusAsync(Guid claimId, UpdateClaimStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> ApproveAsync(Guid claimId, ApproveClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> RejectAsync(Guid claimId, RejectClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> RecordPaymentAsync(Guid claimId, RecordClaimPaymentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(ClaimResponse? Claim, OperationResponse Result)> CloseAsync(Guid claimId, CloseClaimRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
}
