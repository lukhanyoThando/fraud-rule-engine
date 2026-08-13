# Fraud Rule Engine

A .NET 9 fraud detection API that evaluates transaction events against a set of fraud rules and stores the assessment results in SQLite.

**Author:** Lukanyo Nkohla

## Overview

This application processes transaction events, applies fraud rules, and returns a decision such as:

- Clear
- Review
- Block

The system also persists each assessment so it can be retrieved later via API endpoints.

## Architecture

The solution is organized into the following layers:

- Domain: transaction model, customer profile, rule definitions, and fraud decision enum
- Application: fraud evaluation logic and repository contract
- Infrastructure: SQLite persistence and repository implementation
- API: HTTP endpoints for evaluating and retrieving assessments

## Features

- Rule-based fraud scoring
- Transaction-level fraud evaluation
- SQLite persistence
- REST API for storing and retrieving assessments
- Swagger/OpenAPI documentation
- Docker support for running the app in a container

## Project Structure

```text
FraudRuleEngine.Api/
  Controllers/
  Contracts/
  appsettings.json
  Program.cs

FraudRuleEngine.Application/
  Interfaces/
  Services/

FraudRuleEngine.Domain/
  Enums/
  Models/
  Rules/

FraudRuleEngine.Infrastructure/
  Persistence/
  Repositories/

FraudRuleEngine.Tests/
  Application/
```

## Technology Stack

- ASP.NET Core Web API
- .NET 9
- Entity Framework Core
- SQLite
- Swagger/OpenAPI
- Docker

## Prerequisites

Before running the project locally, ensure you have:

- .NET 9 SDK
- Docker Desktop (if using containerized execution)

## Run Locally

From the solution root:

```bash
dotnet restore
dotnet build
dotnet run --project FraudRuleEngine.Api
```

The API will start and create the SQLite database automatically on first run.

## Run with Docker

Docker Desktop must be installed and running before following these steps.

Clone the repository and move into the project folder:

```bash
git clone <repository-url>
cd test
```

Build the Docker image from the solution root, which is the folder containing the `Dockerfile`:

```bash
docker build -t fraud-rule-engine .
```

Start the API container:

```bash
docker run -d -p 8080:8080 --name fraud-rule-engine fraud-rule-engine
```

The `-p 8080:8080` option maps port 8080 on your computer to port 8080 inside the container. Open the Swagger UI at:

```text
http://localhost:8080/swagger
```

Check the container status:

```bash
docker ps
```

View application logs:

```bash
docker logs fraud-rule-engine
```

Stop and remove the container:

```bash
docker stop fraud-rule-engine
docker rm fraud-rule-engine
```

After pulling new code changes, rebuild the image before starting a new container:

```bash
docker rm -f fraud-rule-engine
docker build --no-cache -t fraud-rule-engine .
docker run -d -p 8080:8080 --name fraud-rule-engine fraud-rule-engine
```

### Deploy changes to Docker

Whenever you change the application code, run these commands from the solution root. The image must be rebuilt because Docker does not automatically include changes made after the previous build:

```bash
docker rm -f fraud-rule-engine
docker build -t fraud-rule-engine .
docker run -d -p 8080:8080 --name fraud-rule-engine fraud-rule-engine
```

Use `--no-cache` when you need a completely fresh rebuild:

```bash
docker rm -f fraud-rule-engine
docker build --no-cache -t fraud-rule-engine .
docker run -d -p 8080:8080 --name fraud-rule-engine fraud-rule-engine
```

After deployment, verify the new container and view its logs:

```bash
docker ps
docker logs fraud-rule-engine
```

The SQLite database is stored inside the container at `/app/data/fraud.db`. To keep the database on your computer when the container is removed, mount a local folder:

```bash
docker run -d -p 8080:8080 --name fraud-rule-engine -v "${PWD}/data:/app/data" fraud-rule-engine
```

## API Endpoints

### Evaluate a transaction

```http
POST /api/transactions/evaluate
```

Request body example:

```json
{
  "transactionId": "tx-001",
  "customerId": "cust-123",
  "amount": 2500,
  "currency": "USD",
  "merchant": "Amazon",
  "country": "CA",
  "timestamp": "2026-08-13T12:00:00Z",
  "category": "electronics",
  "deviceId": "device-7"
}
```

### Get an assessment by transaction ID

```http
GET /api/fraudassessments/{transactionId}
```

### Get all assessments

```http
GET /api/fraudassessments
```

## Fraud Rules Used

The application currently evaluates the following rules:

- High amount threshold
- Velocity spike detection
- New country detection
- New merchant detection
- Device change detection

Each matched rule contributes to a total risk score.

## Decision Logic

The service evaluates the total risk score and assigns:

- 80+ -> Block
- 40-79 -> Review
- 0-39 -> Clear

## Database

The application uses SQLite for persistence. In Docker, the database is created in the container at:

```text
/app/data/fraud.db
```

If you run it locally without Docker, the file is created under the API project folder as:

```text
FraudRuleEngine.Api/data/fraud.db
```

## Notes for Interview Assessment

This project demonstrates:

- clean layered architecture
- domain-driven design
- rule-based evaluation
- dependency injection
- API-first design
- persistence integration
- containerization with Docker

These are all strong indicators of a production-minded engineering approach.
