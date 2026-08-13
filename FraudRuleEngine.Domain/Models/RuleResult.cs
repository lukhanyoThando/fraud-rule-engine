namespace FraudRuleEngine.Domain.Models;

public class RuleResult
{
    public string RuleName { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;

    public static RuleResult Match(
        string name,
        int score,
        string reason)
    {
        return new RuleResult
        {
            RuleName = name,
            IsMatch = true,
            Score = score,
            Reason = reason
        };
    }

    public static RuleResult NoMatch(string name)
    {
        return new RuleResult
        {
            RuleName = name,
            IsMatch = false,
            Score = 0,
            Reason = string.Empty
        };
    }
}