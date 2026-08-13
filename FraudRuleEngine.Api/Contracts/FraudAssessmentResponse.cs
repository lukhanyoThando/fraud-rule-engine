namespace FraudRuleEngine.Api.Contracts;

public record FraudAssessmentResponse(
    string TransactionId,
    string CustomerId,
    string Decision,
    int RiskScore,
    IReadOnlyList<string> MatchedRules);