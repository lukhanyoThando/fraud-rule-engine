using FraudRuleEngine.Application.Interfaces;
using FraudRuleEngine.Application.Services;
using FraudRuleEngine.Domain.Rules;
using FraudRuleEngine.Infrastructure.Persistence;
using FraudRuleEngine.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "data"));

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
builder.Services.AddScoped<IFraudRule, RepeatedTransactionRule>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FraudDbContext>();
    dbContext.Database.EnsureCreated();
    EnsureAssessmentTransactionColumns(dbContext);
}

static void EnsureAssessmentTransactionColumns(FraudDbContext dbContext)
{
    var connection = dbContext.Database.GetDbConnection();
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA table_info('FraudAssessments');";

    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            existingColumns.Add(reader.GetString(1));
        }
    }

    foreach (var column in new[]
    {
        (Name: "Amount", Definition: "DECIMAL NOT NULL DEFAULT 0"),
        (Name: "DeviceId", Definition: "TEXT NOT NULL DEFAULT ''"),
        (Name: "TransactionTimestamp", Definition: "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00'")
    })
    {
        if (!existingColumns.Contains(column.Name))
        {
            command.CommandText = $"ALTER TABLE FraudAssessments ADD COLUMN {column.Name} {column.Definition};";
            command.ExecuteNonQuery();
        }
    }

    connection.Close();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();