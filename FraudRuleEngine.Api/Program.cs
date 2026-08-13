using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Application.Services;
using FraudRuleEngine.Domain.Rules;
using FraudRuleEngine.Infrastructure.Persistence;
using FraudRuleEngine.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FraudDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<CustomerProfileRepository>();
builder.Services.AddScoped<FraudAssessmentService>();
builder.Services.AddScoped<IFraudRule, HighAmountRule>();
builder.Services.AddScoped<IFraudRule, VelocitySpikeRule>();
builder.Services.AddScoped<IFraudRule, NewCountryRule>();
builder.Services.AddScoped<IFraudRule, NewMerchantRule>();
builder.Services.AddScoped<IFraudRule, DeviceChangeRule>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FraudDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();