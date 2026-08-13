using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class NewCountryRule : IFraudRule
{
    public string Name => "NewCountry";
    public int Score => 30;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (!string.Equals(
                transaction.Country,
                customer.HomeCountry,
                StringComparison.OrdinalIgnoreCase))
        {
            return RuleResult.Match(
                Name,
                Score,
                "Transaction country differs from the customer's home country.");
        }

        return RuleResult.NoMatch(Name);
    }
}