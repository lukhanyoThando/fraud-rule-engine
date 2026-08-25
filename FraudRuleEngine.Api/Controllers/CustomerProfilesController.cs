using FraudRuleEngine.Api.Contracts;
using FraudRuleEngine.Domain.Models;
using FraudRuleEngine.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FraudRuleEngine.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerProfilesController : ControllerBase
{
    private readonly CustomerProfileRepository repository;

    public CustomerProfilesController(CustomerProfileRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerProfileResponse>> Register(
        CustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TransactionsLast24Hours < 0)
        {
            return BadRequest("TransactionsLast24Hours cannot be negative.");
        }

        if (await repository.ExistsAsync(request.CustomerId, cancellationToken))
        {
            return Conflict($"Customer '{request.CustomerId}' is already registered.");
        }

        var profile = new CustomerProfile
        {
            CustomerId = request.CustomerId,
            HomeCountry = request.HomeCountry,
            PreferredMerchant = request.PreferredMerchant,
            TransactionsLast24Hours = request.TransactionsLast24Hours,
            LastKnownDeviceId = request.LastKnownDeviceId
        };

        await repository.AddAsync(profile, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCustomerId),
            new { customerId = profile.CustomerId },
            ToResponse(profile));
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<CustomerProfileResponse>> GetByCustomerId(
        string customerId,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByCustomerIdAsync(customerId, cancellationToken);

        return profile is null
            ? NotFound()
            : Ok(ToResponse(profile));
    }

    private static CustomerProfileResponse ToResponse(CustomerProfile profile) =>
        new(
            profile.CustomerId,
            profile.HomeCountry,
            profile.PreferredMerchant,
            profile.TransactionsLast24Hours,
            profile.LastKnownDeviceId);
}