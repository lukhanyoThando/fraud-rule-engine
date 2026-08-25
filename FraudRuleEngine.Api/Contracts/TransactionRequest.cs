namespace FraudRuleEngine.Api.Contracts;

public record TransactionRequest
{
    public string? TransactionId { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? Merchant { get; init; }
    public string Country { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
}