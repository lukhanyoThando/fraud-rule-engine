namespace FraudRuleEngine.Domain.Models;

public class CustomerProfile
{
    public string CustomerId { get; set; } = string.Empty;
    public string HomeCountry { get; set; } = string.Empty;
    public string PreferredMerchant { get; set; } = string.Empty;
    public int TransactionsLast24Hours { get; set; }
    public string LastKnownDeviceId { get; set; } = string.Empty;
}