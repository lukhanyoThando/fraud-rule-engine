namespace FraudRuleEngine.Infrastructure.Persistence;

public class CustomerProfileEntity
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string HomeCountry { get; set; } = string.Empty;
    public string PreferredMerchant { get; set; } = string.Empty;
    public int TransactionsLast24Hours { get; set; }
    public string LastKnownDeviceId { get; set; } = string.Empty;
}
