namespace FraudRuleEngine.Infrastructure.Persistence;

public class FraudAssessmentEntity
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string MatchedRulesJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}