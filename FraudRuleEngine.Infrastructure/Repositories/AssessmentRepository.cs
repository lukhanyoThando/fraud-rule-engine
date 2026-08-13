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
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.FraudAssessments.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
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
            matchedRules);
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
                DeserializeMatchedRules(entity.MatchedRulesJson)))
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
