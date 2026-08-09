using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Quotes.DTOs;

namespace TravelInsurance.Application.Quotes.Abstractions;

public interface IQuoteService
{
    Task<(QuoteResponse? Quote, OperationResponse Result)> CreateAsync(CreateQuoteRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(QuoteResponse? Quote, OperationResponse Result)> GetByIdAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> SendAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> AcceptAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> CancelAsync(Guid id, CancelQuoteRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
}
