using FraudRuleEngine.Domain.Models;

namespace FraudRuleEngine.Application.Interfaces;

public interface IAssessmentRepository
{
    Task AddAsync(FraudAssessment assessment, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<int> CountCustomerTransactionsAsync(
        string customerId,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default);
    Task<int> CountMatchingTransactionsAsync(
        string customerId,
        decimal amount,
        string deviceId,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default);
    Task<FraudAssessment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FraudAssessment>> GetAllAsync(CancellationToken cancellationToken = default);
}
