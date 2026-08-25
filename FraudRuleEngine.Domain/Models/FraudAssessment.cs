using FraudRuleEngine.Domain.Enums;

namespace FraudRuleEngine.Domain.Models;

public class FraudAssessment
{
    public string TransactionId { get; }
    public string CustomerId { get; }
    public FraudDecision Decision { get; }
    public int RiskScore { get; }
    public IReadOnlyList<RuleResult> MatchedRules { get; }
    public decimal Amount { get; }
    public string DeviceId { get; }
    public DateTimeOffset Timestamp { get; }

    public FraudAssessment(
        string transactionId,
        string customerId,
        FraudDecision decision,
        int riskScore,
        IReadOnlyList<RuleResult> matchedRules)
        : this(transactionId, customerId, decision, riskScore, matchedRules, 0, string.Empty, DateTimeOffset.MinValue)
    {
    }

    public FraudAssessment(
        string transactionId,
        string customerId,
        FraudDecision decision,
        int riskScore,
        IReadOnlyList<RuleResult> matchedRules,
        decimal amount,
        string deviceId,
        DateTimeOffset timestamp)
    {
        TransactionId = transactionId;
        CustomerId = customerId;
        Decision = decision;
        RiskScore = riskScore;
        MatchedRules = matchedRules;
        Amount = amount;
        DeviceId = deviceId;
        Timestamp = timestamp;
    }
}