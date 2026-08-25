using FraudRuleEngine.Domain.Enums;
using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Domain.Rules;

namespace FraudRuleEngine.Application.Services;

public sealed class FraudAssessmentService
{
    private readonly IEnumerable<IFraudRule> rules;

    public FraudAssessmentService(IEnumerable<IFraudRule> rules)
    {
        this.rules = rules;
    }

    public FraudAssessment Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        var matchedRules = new List<RuleResult>();
        var totalScore = 0;

        foreach (var rule in rules)
        {
            var result = rule.Evaluate(transaction, customer);

            if (result.IsMatch)
            {
                matchedRules.Add(result);
                totalScore += result.Score;
            }
        }

        var decision = totalScore switch
        {
            >= 80 => FraudDecision.Block,
            >= 40 => FraudDecision.Review,
            _ => FraudDecision.Clear
        };

        return new FraudAssessment(
            transaction.TransactionId,
            transaction.CustomerId,
            decision,
            totalScore,
            matchedRules,
            transaction.Amount,
            transaction.DeviceId,
            transaction.Timestamp);
    }
}