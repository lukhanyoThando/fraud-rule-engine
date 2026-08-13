namespace FraudRuleEngine.Domain.Models;

public class TransactionEvent
{
    public string TransactionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}
