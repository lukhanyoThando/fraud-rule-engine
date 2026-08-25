namespace FraudRuleEngine.Api.Contracts;

public record CustomerProfileRequest(
    string CustomerId,
    string HomeCountry,
    string PreferredMerchant,
    int TransactionsLast24Hours,
    string LastKnownDeviceId);