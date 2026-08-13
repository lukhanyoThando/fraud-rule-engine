namespace FraudRuleEngine.Api.Contracts;

public record TransactionRequest(
    string TransactionId,
    string CustomerId,
    decimal Amount,
    string Currency,
    string Merchant,
    string Country,
    DateTimeOffset Timestamp,
    string Category,
    string DeviceId);