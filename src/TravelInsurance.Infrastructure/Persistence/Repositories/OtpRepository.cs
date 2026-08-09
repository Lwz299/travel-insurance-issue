using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class OtpRepository(AppDbContext dbContext) : IOtpRepository
{
    public async Task AddAsync(EmailOtp otp, CancellationToken cancellationToken = default)
    {
        dbContext.EmailOtps.Add(otp);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<EmailOtp?> GetLatestActiveOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        return dbContext.EmailOtps
            .Where(x => x.Email == email && x.Purpose == purpose && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailOtp otp, CancellationToken cancellationToken = default)
    {
        dbContext.EmailOtps.Update(otp);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
