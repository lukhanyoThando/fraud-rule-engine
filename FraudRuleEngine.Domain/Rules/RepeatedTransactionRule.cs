using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class RepeatedTransactionRule : IFraudRule
{
    public string Name => "RepeatedTransaction";
    public int Score => 40;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (customer.MatchingTransactionsLast24Hours >= 5)
        {
            return RuleResult.Match(
                Name,
                Score,
                "More than five transactions with the same amount and device occurred in the last 24 hours.");
        }

        return RuleResult.NoMatch(Name);
    }
}