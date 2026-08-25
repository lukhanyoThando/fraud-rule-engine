FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Test.sln", "."]
COPY ["FraudRuleEngine.Api/FraudRuleEngine.Api.csproj", "FraudRuleEngine.Api/"]
COPY ["FraudRuleEngine.Application/FraudRuleEngine.Application.csproj", "FraudRuleEngine.Application/"]
COPY ["FraudRuleEngine.Domain/FraudRuleEngine.Domain.csproj", "FraudRuleEngine.Domain/"]
COPY ["FraudRuleEngine.Infrastructure/FraudRuleEngine.Infrastructure.csproj", "FraudRuleEngine.Infrastructure/"]
COPY ["FraudRuleEngine.Tests/FraudRuleEngine.Tests.csproj", "FraudRuleEngine.Tests/"]

RUN dotnet restore "Test.sln"

COPY . .
RUN dotnet publish "FraudRuleEngine.Api/FraudRuleEngine.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app
COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "FraudRuleEngine.Api.dll"]
