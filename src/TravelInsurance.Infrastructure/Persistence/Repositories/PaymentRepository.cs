using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class PaymentRepository(AppDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Payments.Include(p => p.Attempts).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    public Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Payments.Include(p => p.Attempts).FirstOrDefaultAsync(p => p.IdempotencyKey == key, cancellationToken);
    public Task<PaymentAttempt?> GetAttemptByProviderReferenceAsync(string providerCode, string providerReference, CancellationToken cancellationToken = default) =>
        dbContext.PaymentAttempts.Include(a => a.Payment).ThenInclude(p => p!.Attempts).FirstOrDefaultAsync(a => a.ProviderCode == providerCode && a.ProviderReference == providerReference, cancellationToken);
    public async Task<IReadOnlyCollection<PaymentAttempt>> GetAttemptsAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        await dbContext.PaymentAttempts.AsNoTracking().Where(a => a.PaymentId == paymentId).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    public void Add(Payment payment) => dbContext.Payments.Add(payment);
    public void AddAttempt(PaymentAttempt attempt) => dbContext.PaymentAttempts.Add(attempt);
    public async Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); return WorkflowSaveResult.Saved; }
        catch (DbUpdateException) { return WorkflowSaveResult.Conflict; }
    }
}
