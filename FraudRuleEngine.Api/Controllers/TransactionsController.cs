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
        var transaction = new TransactionEvent
        {
            TransactionId = request.TransactionId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            Merchant = request.Merchant,
            Country = request.Country,
            Timestamp = request.Timestamp,
            Category = request.Category,
            DeviceId = request.DeviceId
        };

        var customer = await customerProfileRepository.GetByCustomerIdAsync(request.CustomerId)
            ?? new CustomerProfile
            {
                CustomerId = request.CustomerId,
                HomeCountry = "GB",
                PreferredMerchant = "Tesco",
                TransactionsLast24Hours = 3,
                LastKnownDeviceId = "device-1"
            };

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