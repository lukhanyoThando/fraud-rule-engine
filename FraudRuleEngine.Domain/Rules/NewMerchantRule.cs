using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class NewMerchantRule : IFraudRule
{
    public string Name => "NewMerchant";
    public int Score => 20;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (!string.Equals(
                transaction.Merchant,
                customer.PreferredMerchant,
                StringComparison.OrdinalIgnoreCase))
        {
            return RuleResult.Match(
                Name,
                Score,
                "Merchant differs from the customer's preferred merchant.");
        }

        return RuleResult.NoMatch(Name);
    }
}