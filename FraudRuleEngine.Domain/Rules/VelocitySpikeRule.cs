using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class VelocitySpikeRule : IFraudRule
{
    public string Name => "VelocitySpike";
    public int Score => 30;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (customer.TransactionsLast24Hours >= 5)
        {
            return RuleResult.Match(
                Name,
                Score,
                "Customer has made many transactions in the last 24 hours.");
        }

        return RuleResult.NoMatch(Name);
    }
}