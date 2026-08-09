using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Payments.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PaymentAttempt?> GetAttemptByProviderReferenceAsync(string providerCode, string providerReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentAttempt>> GetAttemptsAsync(Guid paymentId, CancellationToken cancellationToken = default);
    void Add(Payment payment);
    void AddAttempt(PaymentAttempt attempt);
    Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
