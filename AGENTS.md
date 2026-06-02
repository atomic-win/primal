# AGENTS.md

## Project Overview

Primal is a .NET 10 backend API for an investment portfolio tracker, built with FastEndpoints, Autofac, and SQLite.

## Architecture

```
src/
  Primal.Domain/          # Domain models, value objects, enums
  Primal.Application/     # Interfaces, business logic (TransactionAmountCalculator)
  Primal.Infrastructure/  # Repositories, API clients, caching, persistence
  Primal.Api/             # FastEndpoints, validators, DTOs, Program.cs
tests/
  Primal.Domain.UnitTests/                # Domain model contract tests
  Primal.Api.UnitTests/                   # Validator tests
  Primal.Infrastructure.IntegrationTests/ # Repository, API client, cache tests
  Primal.E2ETests/                        # End-to-end HTTP tests (primary test suite)
```

## Build and Test

```bash
dotnet build
dotnet test
```

All tests use the [TUnit](https://github.com/thomhurst/TUnit) framework with [Verify](https://github.com/VerifyTests/Verify) snapshot testing.

## Testing Conventions

- **E2E tests are the primary test suite.** Only add unit/integration tests for logic that cannot be exercised via HTTP endpoints.
- Each E2E test is a single class in its own file, named after the expected behavior (e.g., `Returns_400_When_Name_Is_Empty`).
- Use `Verifier.Verify(body)` for assertions — avoid manual `Assert.That` chains in E2E tests.
- E2E tests use `PrimalE2EFactory` with WireMock for external APIs and `FakeTimeProvider` frozen at `2026-06-01`.
- Snapshot files (`.verified.txt`) are committed to the repo and reviewed in PRs.
- Exception tests (`Assert.Throws`) and mock verification (`Received`/`DidNotReceive`) are kept as-is — snapshots don't apply there.
- Use bare `// Arrange`, `// Act`, `// Assert` comments — no extra descriptions.
- Tests run with `[assembly: NotInParallel]` in E2E to avoid port/DB conflicts.

## Code Style

- StyleCop and Meziantou analyzers are enforced as errors.
- Use `string.Empty` instead of `""`.
- Use `StringComparison.Ordinal` for string comparisons.
- Use `CultureInfo.InvariantCulture` for formatting.
- Central package management via `Directory.Packages.props`.
