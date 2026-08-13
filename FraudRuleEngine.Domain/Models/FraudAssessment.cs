using FraudRuleEngine.Domain.Enums;

namespace FraudRuleEngine.Domain.Models;

public class FraudAssessment
{
    public string TransactionId { get; }
    public string CustomerId { get; }
    public FraudDecision Decision { get; }
    public int RiskScore { get; }
    public IReadOnlyList<RuleResult> MatchedRules { get; }

    public FraudAssessment(
        string transactionId,
        string customerId,
        FraudDecision decision,
        int riskScore,
        IReadOnlyList<RuleResult> matchedRules)
    {
        TransactionId = transactionId;
        CustomerId = customerId;
        Decision = decision;
        RiskScore = riskScore;
        MatchedRules = matchedRules;
    }
}