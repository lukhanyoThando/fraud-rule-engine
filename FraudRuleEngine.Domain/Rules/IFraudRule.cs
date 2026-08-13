using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public interface IFraudRule
{
    string Name { get; }
    int Score { get; }

    RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer);
}