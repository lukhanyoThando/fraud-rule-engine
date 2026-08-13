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
