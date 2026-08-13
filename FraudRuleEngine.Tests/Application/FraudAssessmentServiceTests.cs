using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Application.Services;
using FraudRuleEngine.Domain.Enums;
using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Domain.Rules;
using FraudRuleEngine.Infrastructure.Persistence;
using FraudRuleEngine.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FraudRuleEngine.Tests.Application;

public class FraudAssessmentServiceTests
{
    [Fact]
    public void Should_return_review_when_total_score_is_between_40_and_79()
    {
        var rules = new IFraudRule[]
        {
            new HighAmountRule()
        };
        var service = new FraudAssessmentService(rules);
        var transaction = new TransactionEvent
        {
            TransactionId = "tx-1",
            CustomerId = "customer-1",
            Amount = 2000
        };
        var customer = new CustomerProfile();

        var result = service.Evaluate(transaction, customer);

        Assert.Equal(FraudDecision.Review, result.Decision);
        Assert.Equal(40, result.RiskScore);
        Assert.Single(result.MatchedRules);
    }

    [Fact]
    public void Should_return_clear_when_no_rules_match()
    {
        var rules = new IFraudRule[]
        {
            new HighAmountRule()
        };
        var service = new FraudAssessmentService(rules);
        var transaction = new TransactionEvent
        {
            TransactionId = "tx-2",
            CustomerId = "customer-1",
            Amount = 100
        };
        var customer = new CustomerProfile();

        var result = service.Evaluate(transaction, customer);

        Assert.Equal(FraudDecision.Clear, result.Decision);
        Assert.Equal(0, result.RiskScore);
        Assert.Empty(result.MatchedRules);
    }

    [Fact]
    public void Should_return_block_when_total_score_reaches_or_exceeds_80()
    {
        var rules = new IFraudRule[]
        {
            new HighAmountRule(),
            new VelocitySpikeRule(),
            new NewCountryRule(),
            new NewMerchantRule(),
            new DeviceChangeRule()
        };

        var service = new FraudAssessmentService(rules);
        var transaction = new TransactionEvent
        {
            TransactionId = "tx-3",
            CustomerId = "customer-2",
            Amount = 5000,
            Merchant = "Luxury Goods",
            Country = "CA",
            DeviceId = "device-7"
        };
        var customer = new CustomerProfile
        {
            CustomerId = "customer-2",
            TransactionsLast24Hours = 8,
            HomeCountry = "US",
            PreferredMerchant = "Tesco",
            LastKnownDeviceId = "device-9"
        };

        var result = service.Evaluate(transaction, customer);

        Assert.Equal(FraudDecision.Block, result.Decision);
        Assert.True(result.RiskScore >= 80);
        Assert.NotEmpty(result.MatchedRules);
    }

    [Fact]
    public async Task AssessmentRepository_should_persist_and_read_assessment()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FraudDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new FraudDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        await using (var context = new FraudDbContext(options))
        {
            var repository = new AssessmentRepository(context);
            var assessment = new FraudAssessment(
                "tx-123",
                "customer-999",
                FraudDecision.Review,
                55,
                new[]
                {
                    RuleResult.Match("HighAmountRule", 55, "Amount exceeds threshold")
                });

            await repository.AddAsync(assessment);

            var saved = await repository.GetByTransactionIdAsync("tx-123");

            Assert.NotNull(saved);
            Assert.Equal("customer-999", saved!.CustomerId);
            Assert.Equal(FraudDecision.Review, saved.Decision);
            Assert.Equal(55, saved.RiskScore);
        }
    }

    [Fact]
    public async Task AssessmentRepository_should_return_all_assessments()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FraudDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new FraudDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        await using (var context = new FraudDbContext(options))
        {
            var repository = new AssessmentRepository(context);

            await repository.AddAsync(new FraudAssessment(
                "tx-a",
                "customer-a",
                FraudDecision.Clear,
                0,
                Array.Empty<RuleResult>()));

            await repository.AddAsync(new FraudAssessment(
                "tx-b",
                "customer-b",
                FraudDecision.Review,
                45,
                new[]
                {
                    RuleResult.Match("VelocitySpikeRule", 45, "Spike detected")
                }));

            var all = await repository.GetAllAsync();

            Assert.Equal(2, all.Count);
            Assert.Contains(all, x => x.TransactionId == "tx-a");
            Assert.Contains(all, x => x.TransactionId == "tx-b");
        }
    }

    [Fact]
    public async Task CustomerProfileRepository_should_load_customer_from_database()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FraudDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new FraudDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        await using (var context = new FraudDbContext(options))
        {
            var repository = new CustomerProfileRepository(context);
            var profile = await repository.GetByCustomerIdAsync("cust-velocity-001");

            Assert.NotNull(profile);
            Assert.Equal("GB", profile!.HomeCountry);
            Assert.Equal(8, profile.TransactionsLast24Hours);
            Assert.Equal("device-9", profile.LastKnownDeviceId);
        }
    }
}