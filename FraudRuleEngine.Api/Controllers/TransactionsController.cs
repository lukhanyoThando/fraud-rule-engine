using FraudRuleEngine.Api.Contracts;
using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Application.Services;
using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FraudRuleEngine.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly FraudAssessmentService service;
    private readonly IAssessmentRepository repository;
    private readonly CustomerProfileRepository customerProfileRepository;

    public TransactionsController(
        FraudAssessmentService service,
        IAssessmentRepository repository,
        CustomerProfileRepository customerProfileRepository)
    {
        this.service = service;
        this.repository = repository;
        this.customerProfileRepository = customerProfileRepository;
    }

    [HttpPost("evaluate")]
    public async Task<ActionResult<FraudAssessmentResponse>> Evaluate(
        TransactionRequest request)
    {
        var transactionId = string.IsNullOrWhiteSpace(request.TransactionId)
            ? $"tx-{Guid.NewGuid():N}"
            : request.TransactionId;

        if (await repository.ExistsAsync(transactionId))
        {
            return Conflict($"Transaction '{transactionId}' has already been evaluated.");
        }

        var customer = await customerProfileRepository.GetByCustomerIdAsync(
            request.CustomerId,
            HttpContext.RequestAborted);

        if (customer is null)
        {
            return NotFound($"Customer '{request.CustomerId}' is not registered.");
        }

        var transaction = new TransactionEvent
        {
            TransactionId = transactionId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            Merchant = string.IsNullOrWhiteSpace(request.Merchant)
                ? customer.PreferredMerchant
                : request.Merchant,
            Country = request.Country,
            Timestamp = DateTimeOffset.UtcNow,
            Category = request.Category,
            DeviceId = request.DeviceId
        };

        customer.TransactionsLast24Hours =
            await repository.CountCustomerTransactionsAsync(
                transaction.CustomerId,
                transaction.Timestamp);

        customer.MatchingTransactionsLast24Hours =
            await repository.CountMatchingTransactionsAsync(
                transaction.CustomerId,
                transaction.Amount,
                transaction.DeviceId,
                transaction.Timestamp);

        var result = service.Evaluate(transaction, customer);
        await repository.AddAsync(result);

        var response = new FraudAssessmentResponse(
            result.TransactionId,
            result.CustomerId,
            result.Decision.ToString(),
            result.RiskScore,
            result.MatchedRules
                .Select(rule => rule.RuleName)
                .ToList());

        return Ok(response);
    }
}