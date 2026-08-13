using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class HighAmountRule : IFraudRule
{
    public string Name => "HighAmount";
    public int Score => 40;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (transaction.Amount > 1000)
        {
            return RuleResult.Match(
                Name,
                Score,
                "Transaction amount is above threshold.");
        }

        return RuleResult.NoMatch(Name);
    }
}