# Fraud Rule Engine

**Author:** Lukanyo Tando Nkohla

A .NET 9 Web API that evaluates transactions against fraud rules, stores customer profiles and assessments in SQLite, and returns `Clear`, `Review`, or `Block` decisions.

## What This Project Demonstrates

- Layered architecture
- Domain-driven fraud rules
- Dependency injection
- ASP.NET Core Web API endpoints
- Entity Framework Core with SQLite
- Automatic transaction history checks
- Customer-specific fraud comparisons
- Swagger/OpenAPI documentation
- Docker and Docker Compose
- Automated xUnit tests

## Technology

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- SQLite
- Swashbuckle Swagger
- xUnit
- Docker

## Architecture

The solution separates responsibilities into five projects:

```text
FraudRuleEngine.Api/
  Controllers/       HTTP endpoints
  Contracts/         Request and response DTOs
  Program.cs         Dependency injection and application startup

FraudRuleEngine.Application/
  Interfaces/        Repository contracts
  Services/          Fraud evaluation orchestration

FraudRuleEngine.Domain/
  Enums/             Fraud decisions
  Models/            Transaction, customer, assessment, and rule result models
  Rules/             Individual fraud rules

FraudRuleEngine.Infrastructure/
  Persistence/       EF Core DbContext and database entities
  Repositories/      SQLite repository implementations

FraudRuleEngine.Tests/
  Application/       Service and repository tests
  Domain/            Individual rule tests

scripts/
  reset-local-database.ps1
  reset-docker-database.ps1
```

### Request Flow

1. A customer is registered through `POST /api/customers`.
2. A transaction is submitted through `POST /api/transactions/evaluate`.
3. The API finds the profile using `customerId`.
4. The server generates a transaction ID if one was not supplied.
5. The server records the current UTC timestamp.
6. The repository counts the customer's recent transactions.
7. All registered fraud rules evaluate the transaction.
8. Rule scores are added together.
9. The result is classified as `Clear`, `Review`, or `Block`.
10. The assessment and transaction details are saved in SQLite.
11. The API returns the assessment response.

## Database Design

The application uses two SQLite tables.

### CustomerProfiles

Stores the baseline behavior for each customer:

- `CustomerId`
- `HomeCountry`
- `PreferredMerchant`
- `TransactionsLast24Hours` initial profile value
- `LastKnownDeviceId`

The application seeds one customer:

```text
CustomerId:              cust-velocity-001
HomeCountry:             GB
PreferredMerchant:       Tesco
TransactionsLast24Hours: 8
LastKnownDeviceId:       device-9
```

New customers can be registered through the API. Their profiles are independent; transactions for one customer never affect another customer.

### FraudAssessments

Stores each evaluated transaction and its result:

- Transaction ID
- Customer ID
- Amount
- Device ID
- Server transaction timestamp
- Decision
- Risk score
- Matched rules
- Assessment creation time

This table supplies the history used by `VelocitySpike` and `RepeatedTransaction`.

## Fraud Rules

Every evaluation runs all six rules. There is no manual rule-selection step.

| Rule | Trigger | Score |
|---|---|---:|
| `HighAmount` | Amount is greater than `1000` | 40 |
| `VelocitySpike` | Customer has at least 5 transactions in the previous 24 hours | 30 |
| `NewCountry` | Transaction country differs from the customer's home country | 30 |
| `NewMerchant` | Transaction merchant differs from the preferred merchant | 20 |
| `DeviceChange` | Transaction device differs from the customer's known device | 30 |
| `RepeatedTransaction` | At least 5 previous transactions have the same customer, amount, and device within 24 hours | 40 |

The current transaction is not included in the history count before evaluation. Therefore, the sixth transaction triggers the history-based rules.

### VelocitySpike

`VelocitySpike` is automatic. The API counts previous `FraudAssessments` for the same customer during the previous 24 hours. The registered profile's `TransactionsLast24Hours` value is retained as profile data, but it does not need to be updated manually for the runtime velocity check.

### RepeatedTransaction

`RepeatedTransaction` is narrower than velocity. It counts only previous transactions matching all of these values:

- Same `customerId`
- Same `amount`
- Same `deviceId`
- Transaction timestamp within the previous 24 hours

On the sixth identical request, both `VelocitySpike` and `RepeatedTransaction` can match. Their combined score is `70`, which produces `Review`.

### DeviceChange

The submitted `deviceId` is compared with the `LastKnownDeviceId` stored for the customer identified by `customerId`. The application does not automatically replace the known device after an alert. This prevents an untrusted device from becoming trusted simply because it was submitted.

## Decision Thresholds

| Total score | Decision |
|---:|---|
| 0-39 | `Clear` |
| 40-79 | `Review` |
| 80 or more | `Block` |

An alert can be present while the overall decision is `Clear`. For example, `DeviceChange` adds 30 points, so it is returned in `matchedRules` but does not reach `Review` alone.

## Prerequisites

For local execution:

- .NET 9 SDK
- PowerShell, Command Prompt, or a compatible terminal

For Docker execution:

- Docker Desktop with Docker Compose

## Run Locally

Run from the solution root, the folder containing `Test.sln`:

```powershell
dotnet restore
dotnet build Test.sln
dotnet run --project FraudRuleEngine.Api --launch-profile http
```

The API listens on:

```text
http://localhost:5167
```

Swagger is available at:

```text
http://localhost:5167/swagger
```

Use the `http` launch profile. Running without a launch profile may use port `5000`, which can already be occupied.

The first startup creates the local database automatically at:

```text
FraudRuleEngine.Api/data/fraud.db
```

## Run with Docker Compose

Run from the solution root:

```powershell
docker compose up --build -d
```

The Docker API listens on:

```text
http://localhost:8080
```

Swagger is available at:

```text
http://localhost:8080/swagger
```

The Compose file maps the host folder `./data` to `/app/data` inside the container. The Docker database is therefore stored on the host at:

```text
data/fraud.db
```

Useful Docker commands:

```powershell
docker compose ps
docker compose logs -f fraud-rule-engine
docker compose restart
docker compose down
```

After code changes, rebuild the image:

```powershell
docker compose up --build -d
```

For a completely fresh image build:

```powershell
docker compose build --no-cache
docker compose up -d
```

The application data remains after `docker compose down` because it is stored in the host-mounted `data` folder.

## API Usage

### Register a Customer

```http
POST http://localhost:5167/api/customers
```

Request:

```json
{
  "customerId": "luks_001",
  "homeCountry": "ZA",
  "preferredMerchant": "Yoco",
  "transactionsLast24Hours": 0,
  "lastKnownDeviceId": "device-1"
}
```

Response status: `201 Created`.

The `transactionsLast24Hours` value should normally start at `0`. The application calculates current velocity from saved transactions.

Customer IDs must be unique. Registering the same customer twice returns `409 Conflict`.

### Get a Customer

```http
GET http://localhost:5167/api/customers/luks_001
```

An unknown customer returns `404 Not Found`.

### Evaluate a Transaction

```http
POST http://localhost:5167/api/transactions/evaluate
```

Request:

```json
{
  "customerId": "luks_001",
  "amount": 2500,
  "currency": "Rand",
  "merchant": "Yoco",
  "country": "ZA",
  "category": "shoprite",
  "deviceId": "device-1"
}
```

`transactionId` is optional. If omitted, the server generates a unique ID. The server also assigns the current UTC time; clients do not need to send a timestamp.

`customerId`, `country`, and `deviceId` are used with the registered customer profile. `merchant` is optional and defaults to the customer's preferred merchant; it only triggers `NewMerchant` when a different value is provided. `currency` and `category` are currently stored in the incoming domain transaction but do not affect the current rule scores.

Example response:

```json
{
  "transactionId": "tx-generated-by-server",
  "customerId": "luks_001",
  "decision": "Review",
  "riskScore": 40,
  "matchedRules": [
    "HighAmount"
  ]
}
```

With the customer profile above, `ZA` and `device-1` match the customer's profile. Amount `2500` triggers `HighAmount`.

### Get One Assessment

Use the transaction ID returned by the evaluation response:

```http
GET http://localhost:5167/api/fraudassessments/{transactionId}
```

An unknown transaction ID returns `404 Not Found`.

### Get All Assessments

```http
GET http://localhost:5167/api/fraudassessments
```

## Testing Scenarios

Use Swagger or a REST client. Start each test with a new customer ID, or reset the database first. The examples below use local port `5167`; replace it with `8080` when using Docker.

### Clear

Register a customer with matching values and `transactionsLast24Hours: 0`, then evaluate an amount of `100` using the same country, merchant, and device.

Expected: `Clear`, score `0`, no matched rules.

### High Amount

Using a clean customer profile, set:

```json
"amount": 1500
```

Expected: `Review`, score `40`, matched rule `HighAmount`.

An amount of exactly `1000` does not trigger the rule.

### Velocity Spike

1. Register a customer with `transactionsLast24Hours: 0`.
2. Send six transactions for the same `customerId`.
3. Keep the country, merchant, and device valid for the profile.
4. Use different amounts such as `100`, `101`, `102`, `103`, `104`, and `105` so `RepeatedTransaction` does not match.
5. Omit `transactionId` and timestamp.

Expected:

```text
Requests 1-5: Clear, score 0
Request 6:    Clear, score 30, matched rule VelocitySpike
```

The response is `Clear` because the velocity alert contributes 30 points and `Review` begins at 40.

### New Country

Register a customer with `homeCountry: ZA`, then submit a transaction with:

```json
"country": "US"
```

Expected: `Clear`, score `30`, matched rule `NewCountry`.

### New Merchant

Register a customer with `preferredMerchant: Yoco`, then submit:

```json
"merchant": "Amazon"
```

Expected: `Clear`, score `20`, matched rule `NewMerchant`.

If merchant is omitted, the preferred merchant is used and `NewMerchant` does not match.

### Device Change

Register a customer with:

```json
"lastKnownDeviceId": "device-1"
```

Then submit:

```json
"deviceId": "device-new"
```

Expected: `Clear`, score `30`, matched rule `DeviceChange`.

The known device is not automatically changed after this alert.

### Repeated Transaction

1. Register a new customer with `transactionsLast24Hours: 0`.
2. Send the same amount, device, customer, country, and merchant six times.
3. Omit `transactionId` and timestamp so the server generates them.

Expected:

```text
Requests 1-5: Clear, score 0
Request 6:    Review, score 70
```

The sixth request matches both `VelocitySpike` (30 points) and `RepeatedTransaction` (40 points).

### Block

Register a customer with home country `ZA`, preferred merchant `Yoco`, and known device `device-1`. Submit a transaction with:

```text
amount: 1500
country: US
merchant: Amazon
deviceId: device-new
```

The score is:

```text
HighAmount    40
NewCountry    30
NewMerchant   20
DeviceChange  30
Total        120 -> Block
```

Expected matched rules:

```json
[
  "HighAmount",
  "NewCountry",
  "NewMerchant",
  "DeviceChange"
]
```

## Reset the Database

Stop the application before deleting its database.

### Reset Local SQLite

Run the reset script from the solution root:

```powershell
.\scripts\reset-local-database.ps1
```

The script stops running local API processes and removes the local database. The manual equivalent is:

```powershell
Remove-Item .\FraudRuleEngine.Api\data\fraud.db -Force -ErrorAction SilentlyContinue
```

Restart the application:

```powershell
dotnet run --project FraudRuleEngine.Api --launch-profile http
```

The application recreates the database, tables, and seeded customer automatically. Custom customers and assessments are removed.

### Reset Docker SQLite

Run the reset script from the solution root:

```powershell
.\scripts\reset-docker-database.ps1
```

The script stops the Compose service and removes the host-mounted database. The manual equivalent is:

```powershell
docker compose down
Remove-Item .\data\fraud.db -Force -ErrorAction SilentlyContinue
docker compose up --build -d
```

The Docker database is recreated at `data/fraud.db` with the seeded customer. This does not delete the Docker image.

## Run Automated Tests

Run all tests from the solution root:

```powershell
dotnet test Test.sln
```

Run tests without rebuilding:

```powershell
dotnet test FraudRuleEngine.Tests/FraudRuleEngine.Tests.csproj --no-build
```

Run only the domain tests:

```powershell
dotnet test FraudRuleEngine.Tests/FraudRuleEngine.Tests.csproj --filter FullyQualifiedName~Domain
```

Run only application tests:

```powershell
dotnet test FraudRuleEngine.Tests/FraudRuleEngine.Tests.csproj --filter FullyQualifiedName~Application
```

The current test project contains 8 tests covering:

- Clear, Review, and Block scoring
- High amount behavior
- Assessment persistence and retrieval
- Reading the seeded customer profile

## NuGet Sources and Package Cleanup

The project references only these required packages:

```text
Microsoft.AspNetCore.OpenApi 9.0.3
Swashbuckle.AspNetCore 7.2.0
Microsoft.EntityFrameworkCore 9.0.3
Microsoft.EntityFrameworkCore.Sqlite 9.0.3
```

No Woolworths package is referenced by the project files. The previous restore warnings came from private Woolworths feeds configured in the user's global NuGet configuration. Those sources have been disabled:

```text
Woolworths Dev
CAD Stable
```

Check configured sources:

```powershell
dotnet nuget list source
```

The public `NuGet` source should remain enabled. If package restore still reports a private source on another machine, disable the source by name:

```powershell
dotnet nuget disable source "Woolworths Dev"
dotnet nuget disable source "CAD Stable"
```

## Troubleshooting

### Port 5167 is already in use

Stop the previous API process or use another port. The recommended local command is:

```powershell
dotnet run --project FraudRuleEngine.Api --launch-profile http
```

### Port 5000 is already in use

Do not use `--no-launch-profile` for normal local execution. That command can select the default port 5000 instead of the configured port 5167.

### Database cannot be opened

The API creates its `data` folder during startup. If the database is locked, stop all API instances before restarting. Do not run the local API and Docker API against the same database folder at the same time.

### Customer not found

Register the customer first with `POST /api/customers`. The evaluation endpoint returns `404 Not Found` for an unknown `customerId`.

### Duplicate transaction

If you explicitly send a transaction ID that already exists, the API returns `409 Conflict`. Omit the field to let the server generate a new ID.

## Interview Summary

This application receives a transaction, identifies the customer's stored baseline using `customerId`, checks the transaction against six independently registered rules, calculates a total score, persists the assessment, and returns a decision.

The most important design point is that customer data and transaction history are separate concerns. `CustomerProfiles` stores what is normal for a customer. `FraudAssessments` stores what has happened. Profile comparisons detect country, merchant, and device changes; transaction history detects velocity and repeated activity.

The system is intentionally small, but the boundaries make it straightforward to extend with authentication, a production database, migrations, richer validation, event processing, or additional fraud rules.
