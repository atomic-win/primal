# Investment Portfolio Tracker API

Backend API Server for [Investment Portfolio Tracker WebApp](https://github.com/atomic-win/investment-portfolio-tracker-webapp)

## Tech Stack

- .NET 10, FastEndpoints, Autofac, SQLite, HybridCache
- TUnit + Verify for snapshot testing
- WireMock.Net for external API mocking in E2E tests

## Getting Started

```bash
dotnet build
dotnet test
```

## Project Structure

| Project                                                            | Description                            |
| ------------------------------------------------------------------ | -------------------------------------- |
| `src/InvestmentPortfolioTracker.Api`                               | HTTP endpoints, validators, DTOs       |
| `src/InvestmentPortfolioTracker.Core`                              | Interfaces, business logic             |
| `src/InvestmentPortfolioTracker.Domain`                            | Domain models, value objects           |
| `src/InvestmentPortfolioTracker.Infrastructure`                    | Repositories, API clients, persistence |
| `tests/InvestmentPortfolioTracker.E2ETests`                        | End-to-end HTTP tests (primary suite)  |
| `tests/InvestmentPortfolioTracker.Domain.UnitTests`                | Domain model contract tests            |
| `tests/InvestmentPortfolioTracker.Api.UnitTests`                   | Validator tests                        |
| `tests/InvestmentPortfolioTracker.Infrastructure.IntegrationTests` | Repository and API client tests        |

## License

See [LICENSE](LICENSE).
