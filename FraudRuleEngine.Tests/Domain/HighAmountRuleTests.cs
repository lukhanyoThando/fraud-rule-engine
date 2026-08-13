using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Domain.Rules;

namespace FraudRuleEngine.Tests.Domain;

public class HighAmountRuleTests
{
    [Fact]
    public void Should_match_when_amount_is_above_threshold()
    {
        var rule = new HighAmountRule();
        var transaction = new TransactionEvent
        {
            Amount = 2000
        };
        var customer = new CustomerProfile();

        var result = rule.Evaluate(transaction, customer);

        Assert.True(result.IsMatch);
        Assert.Equal("HighAmount", result.RuleName);
        Assert.Equal(40, result.Score);
    }

    [Fact]
    public void Should_not_match_when_amount_is_at_or_below_threshold()
    {
        var rule = new HighAmountRule();
        var transaction = new TransactionEvent
        {
            Amount = 1000
        };
        var customer = new CustomerProfile();

        var result = rule.Evaluate(transaction, customer);

        Assert.False(result.IsMatch);
        Assert.Equal(0, result.Score);
    }
}