using FraudRuleEngine.Api.Contracts;
using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace FraudRuleEngine.Api.Controllers;

[ApiController]
[Route("api/fraudassessments")]
public class FraudAssessmentsController : ControllerBase
{
    private readonly IAssessmentRepository repository;

    public FraudAssessmentsController(IAssessmentRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("{transactionId}")]
    public async Task<ActionResult<FraudAssessmentResponse>> GetByTransactionId(string transactionId)
    {
        var result = await repository.GetByTransactionIdAsync(transactionId);

        if (result is null)
        {
            return NotFound();
        }

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FraudAssessmentResponse>>> GetAll()
    {
        var all = await repository.GetAllAsync();

        var response = all
            .Select(result => new FraudAssessmentResponse(
                result.TransactionId,
                result.CustomerId,
                result.Decision.ToString(),
                result.RiskScore,
                result.MatchedRules
                    .Select(rule => rule.RuleName)
                    .ToList()))
            .ToList();

        return Ok(response);
    }
}
