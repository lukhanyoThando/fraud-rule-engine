using Microsoft.EntityFrameworkCore;

namespace FraudRuleEngine.Infrastructure.Persistence;

public class FraudDbContext : DbContext
{
    public FraudDbContext(DbContextOptions<FraudDbContext> options)
        : base(options)
    {
    }

    public DbSet<FraudAssessmentEntity> FraudAssessments =>
        Set<FraudAssessmentEntity>();

    public DbSet<CustomerProfileEntity> CustomerProfiles =>
        Set<CustomerProfileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerProfileEntity>().HasData(
            new CustomerProfileEntity
            {
                Id = 1,
                CustomerId = "cust-velocity-001",
                HomeCountry = "GB",
                PreferredMerchant = "Tesco",
                TransactionsLast24Hours = 8,
                LastKnownDeviceId = "device-9"
            });
    }
}