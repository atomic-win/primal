# primal

Backend API Server for [Investment Portfolio Tracker](https://github.com/atomic-win/investment-portfolio-tracker)

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

| Project | Description |
|---------|-------------|
| `src/Primal.Api` | HTTP endpoints, validators, DTOs |
| `src/Primal.Application` | Interfaces, business logic |
| `src/Primal.Domain` | Domain models, value objects |
| `src/Primal.Infrastructure` | Repositories, API clients, persistence |
| `tests/Primal.E2ETests` | End-to-end HTTP tests (primary suite) |
| `tests/Primal.Domain.UnitTests` | Domain model contract tests |
| `tests/Primal.Api.UnitTests` | Validator tests |
| `tests/Primal.Infrastructure.IntegrationTests` | Repository and API client tests |

## License & Usage Notice

This repository is made public **for viewing purposes only**.  
At this time, the code is **not licensed for copying, modification, or redistribution**.  

You may **browse and read** the source code, but you **do not have permission** to:  
- Copy or reuse the code in your own projects  
- Fork or redistribute the repository  
- Modify and share derivative works  

A license may be added in the future, but until then, **all rights are reserved**.
