using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FraudRuleEngine.Infrastructure.Repositories;

public sealed class CustomerProfileRepository
{
    private readonly FraudDbContext context;

    public CustomerProfileRepository(FraudDbContext context)
    {
        this.context = context;
    }

    public async Task<CustomerProfile?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var entity = await context.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return ToDomain(entity);
    }

    public Task<bool> ExistsAsync(string customerId, CancellationToken cancellationToken = default) =>
        context.CustomerProfiles.AnyAsync(x => x.CustomerId == customerId, cancellationToken);

    public async Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await context.CustomerProfiles.AddAsync(new CustomerProfileEntity
        {
            CustomerId = profile.CustomerId,
            HomeCountry = profile.HomeCountry,
            PreferredMerchant = profile.PreferredMerchant,
            TransactionsLast24Hours = profile.TransactionsLast24Hours,
            LastKnownDeviceId = profile.LastKnownDeviceId
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static CustomerProfile ToDomain(CustomerProfileEntity entity)
    {
        return new CustomerProfile
        {
            CustomerId = entity.CustomerId,
            HomeCountry = entity.HomeCountry,
            PreferredMerchant = entity.PreferredMerchant,
            TransactionsLast24Hours = entity.TransactionsLast24Hours,
            LastKnownDeviceId = entity.LastKnownDeviceId
        };
    }
}
