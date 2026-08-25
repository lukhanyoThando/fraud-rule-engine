namespace FraudRuleEngine.Api.Contracts;

public record CustomerProfileResponse(
    string CustomerId,
    string HomeCountry,
    string PreferredMerchant,
    int TransactionsLast24Hours,
    string LastKnownDeviceId);