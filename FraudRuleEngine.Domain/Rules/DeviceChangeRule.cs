using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Domain.Rules;

public sealed class DeviceChangeRule : IFraudRule
{
    public string Name => "DeviceChange";
    public int Score => 30;

    public RuleResult Evaluate(
        TransactionEvent transaction,
        CustomerProfile customer)
    {
        if (!string.Equals(
                transaction.DeviceId,
                customer.LastKnownDeviceId,
                StringComparison.OrdinalIgnoreCase))
        {
            return RuleResult.Match(
                Name,
                Score,
                "Transaction uses a device not previously associated with the customer.");
        }

        return RuleResult.NoMatch(Name);
    }
}