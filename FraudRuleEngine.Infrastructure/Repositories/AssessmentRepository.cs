using System.Text.Json;
using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Domain.Enums;
using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FraudRuleEngine.Infrastructure.Repositories;

public sealed class AssessmentRepository : IAssessmentRepository
{
    private readonly FraudDbContext context;

    public AssessmentRepository(FraudDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(FraudAssessment assessment, CancellationToken cancellationToken = default)
    {
        var entity = new FraudAssessmentEntity
        {
            TransactionId = assessment.TransactionId,
            CustomerId = assessment.CustomerId,
            Decision = assessment.Decision.ToString(),
            RiskScore = assessment.RiskScore,
            MatchedRulesJson = JsonSerializer.Serialize(assessment.MatchedRules),
            CreatedAt = DateTimeOffset.UtcNow,
            Amount = assessment.Amount,
            DeviceId = assessment.DeviceId,
            TransactionTimestamp = assessment.Timestamp
        };

        await context.FraudAssessments.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(string transactionId, CancellationToken cancellationToken = default) =>
        context.FraudAssessments.AnyAsync(x => x.TransactionId == transactionId, cancellationToken);

    public Task<int> CountCustomerTransactionsAsync(
        string customerId,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default)
    {
        var windowStart = timestamp.AddHours(-24);

        return CountCustomerTransactionsAsync(
            customerId,
            timestamp,
            windowStart,
            cancellationToken);
    }

    private async Task<int> CountCustomerTransactionsAsync(
        string customerId,
        DateTimeOffset timestamp,
        DateTimeOffset windowStart,
        CancellationToken cancellationToken)
    {
        var timestamps = await context.FraudAssessments
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .Select(x => x.TransactionTimestamp)
            .ToListAsync(cancellationToken);

        return timestamps.Count(transactionTimestamp =>
            transactionTimestamp >= windowStart && transactionTimestamp <= timestamp);
    }

    public Task<int> CountMatchingTransactionsAsync(
        string customerId,
        decimal amount,
        string deviceId,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default)
    {
        var windowStart = timestamp.AddHours(-24);

        return CountMatchingTransactionsAsync(
            customerId,
            amount,
            deviceId,
            timestamp,
            windowStart,
            cancellationToken);
    }

    private async Task<int> CountMatchingTransactionsAsync(
        string customerId,
        decimal amount,
        string deviceId,
        DateTimeOffset timestamp,
        DateTimeOffset windowStart,
        CancellationToken cancellationToken)
    {
        var candidates = await context.FraudAssessments
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .Select(x => new { x.Amount, x.DeviceId, x.TransactionTimestamp })
            .ToListAsync(cancellationToken);

        return candidates.Count(candidate =>
            candidate.Amount == amount
                && string.Equals(candidate.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase)
                && candidate.TransactionTimestamp >= windowStart
                && candidate.TransactionTimestamp <= timestamp);
    }

    public async Task<FraudAssessment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var entity = await context.FraudAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var matchedRules = DeserializeMatchedRules(entity.MatchedRulesJson);

        return new FraudAssessment(
            entity.TransactionId,
            entity.CustomerId,
            Enum.Parse<FraudDecision>(entity.Decision),
            entity.RiskScore,
            matchedRules,
            entity.Amount,
            entity.DeviceId,
            entity.TransactionTimestamp);
    }

    public async Task<IReadOnlyList<FraudAssessment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await context.FraudAssessments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => new FraudAssessment(
                entity.TransactionId,
                entity.CustomerId,
                Enum.Parse<FraudDecision>(entity.Decision),
                entity.RiskScore,
                DeserializeMatchedRules(entity.MatchedRulesJson),
                entity.Amount,
                entity.DeviceId,
                entity.TransactionTimestamp))
            .ToList();
    }

    private static List<RuleResult> DeserializeMatchedRules(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<RuleResult>();
        }

        return JsonSerializer.Deserialize<List<RuleResult>>(json) ?? new List<RuleResult>();
    }
}
