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
bruno/                    # Bruno API collection (YAML format)
```

## Build and Test

```bash
dotnet build
dotnet test
```

All tests use the [TUnit](https://github.com/thomhurst/TUnit) framework with [Verify](https://github.com/VerifyTests/Verify) snapshot testing.

## Adding a New Endpoint

Each endpoint lives in its own folder under `Primal.Api/{Resource}/{Action}/` with three files:

1. **Request** (`{Action}Request.cs`): Record with `[FromClaim]` for UserId, route/body params.
2. **Validator** (`{Action}Validator.cs`): Extends `Validator<TRequest>`. Use `ErrorCodes` and `ErrorMessages` constants. For long validators, extract rules into private methods (MA0051 enforces max 60 lines per method).
3. **Endpoint** (`{Action}Endpoint.cs`): Extends `Endpoint<TRequest>` or `Endpoint<TRequest, TResponse>`. Use `[Http{Verb}("route")]` attribute. Use `ErrorFactory` for 404s.

When adding CRUD operations, update all layers:
- `Primal.Application`: Add method to the repository interface (e.g., `IAssetItemRepository`).
- `Primal.Infrastructure`: Implement in both the repository (e.g., `AssetItemRepository`) and its cached decorator (e.g., `CachedAssetItemRepository`). Cached decorators must invalidate relevant cache keys after mutations.
- `Primal.Api/Errors`: Add new `ErrorCodes` and `ErrorMessages` constants if needed.
- `bruno/`: Add a `.yml` request file for the new endpoint.
- `TESTS.md`: Add test scenarios for the new endpoint and update the summary table.
- `AGENTS.md`: Update this file to reflect any new conventions, patterns, or behavioral changes.

### Error Handling Pattern

- Validation errors: Use `ErrorCodes.*` and `ErrorMessages.*` constants in validators.
- Not-found errors: Use `ErrorFactory.*NotFound()` in endpoint handlers, then `this.ThrowError(error, StatusCodes.Status404NotFound)`.

### Repository Pattern

- Repositories use Dapper with raw SQL against SQLite.
- Format GUIDs as uppercase `"D"` format with `CultureInfo.InvariantCulture`: `id.Value.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant()`.
- Set `UpdatedAt` on mutations using `this.timeProvider.GetUtcNow().ToString("O")`.
- Every repository has a `CachedXxxRepository` decorator using `HybridCache`. Mutations must call `RemoveAsync` on affected cache keys.

### Edit Endpoint Pattern

For PATCH endpoints that partially update an entity:
- Look up the existing entity first; return 404 if not found.
- Skip the DB update if nothing has changed (compare with `StringComparison.Ordinal` for strings).
- Build a new domain object with merged values and pass it to `UpdateAsync`.
- Return `204 No Content`.

## Testing Conventions

- **E2E tests are the primary test suite.** Only add unit/integration tests for logic that cannot be exercised via HTTP endpoints.
- Each E2E test is a single class in its own file, named after the expected behavior (e.g., `Returns_400_When_Name_Is_Empty`).
- Use `Verifier.Verify(body)` for assertions — avoid manual `Assert.That` chains in E2E tests.
- E2E tests use `PrimalE2EFactory` with WireMock for external APIs and `FakeTimeProvider` frozen at `2026-06-01`.
- Snapshot files (`.verified.txt`) are committed to the repo and reviewed in PRs. Run new tests once to generate `.received.txt`, verify the content, then rename to `.verified.txt`.
- Exception tests (`Assert.Throws`) and mock verification (`Received`/`DidNotReceive`) are kept as-is — snapshots don't apply there.
- Use bare `// Arrange`, `// Act`, `// Assert` comments — no extra descriptions.
- Tests run with `[assembly: NotInParallel]` in E2E to avoid port/DB conflicts.
- Helper methods like `AddAssetItemAsync` and `AddTransactionAsync` are in `HttpClientExtensions.cs` for reuse across tests.
- TUnit filter syntax for running specific tests: `--treenode-filter "/Primal.E2ETests/Primal.E2ETests.Namespace.ClassName/**"`.
- After adding or modifying tests, run `dotnet test` and ensure the total count in `TESTS.md` stays accurate.

## Code Style

- StyleCop and Meziantou analyzers are enforced as errors — `dotnet build` will fail on violations.
- Meziantou MA0051 limits methods to 60 lines — extract into private methods if needed.
- Use `string.Empty` instead of `""`.
- Use `StringComparison.Ordinal` for string comparisons.
- Use `CultureInfo.InvariantCulture` for formatting.
- Use `internal sealed` for all non-public classes.
- Central package management via `Directory.Packages.props`.

## External APIs

- **AlphaVantage** (`AlphaVantageApiClient`): Single client for stocks, ETFs, and forex. Uses CSV endpoints (`SYMBOL_SEARCH`, `TIME_SERIES_DAILY`, `FX_DAILY`). Implements both `IAssetApiClient<Stock>` and `IForexApiClient`.
- **MutualFund API** (`MutualFundApiClient`): Indian mutual fund data from `api.mfapi.in`. Implements `IAssetApiClient<MutualFund>`.
- Both API clients are wrapped with caching decorators (`CachedAssetApiClient<T>`, `CachedForexApiClient`) and rate persistence via `RateRepository`.
- Config keys: `InvestmentSettings:AlphaVantageApiKey`, `InvestmentSettings:AlphaVantageBaseUrl`, `InvestmentSettings:MutualFundApiBaseUrl`.

### Asset Type Derivation

- `Stock` and `ETF` asset types are derived from AlphaVantage `SYMBOL_SEARCH` response `type` field (`Equity` → `Stock`, `ETF` → `ETF`). Users send `AssetType=Stock` in the request; the actual type is resolved server-side.
- `ETF` cannot be set directly in requests — the validator rejects it.
- Unsupported symbol types (anything other than `Equity` or `ETF`) throw `NotSupportedException`.
- ETF behaves identically to Stock at runtime: same allowed transaction types (Buy, Sell, Dividend), same price lookup via `AlphaVantageApiClient`, and same invested-value calculation (Buy minus Sell) in valuations.

## Key Files Reference

| File | Purpose |
|------|---------|
| `src/Primal.Api/Errors/ErrorCodes.cs` | All validation error code constants |
| `src/Primal.Api/Errors/ErrorMessages.cs` | All validation error message constants |
| `src/Primal.Api/Errors/ErrorFactory.cs` | Factory for not-found `ValidationFailure` instances |
| `tests/Primal.E2ETests/PrimalE2EFactory.cs` | Test server factory with WireMock, FakeTimeProvider, JWT auth |
| `tests/Primal.E2ETests/WireMockServerExtensions.cs` | WireMock stub helpers for AlphaVantage and MutualFund APIs |
| `tests/Primal.E2ETests/HttpClientExtensions.cs` | Helper methods for creating test data |
| `TESTS.md` | All test scenarios with counts — must be kept in sync |

## Maintenance

- **Keep `AGENTS.md` up to date.** After every code change, update this file to reflect new conventions, patterns, asset type behavior, or architectural decisions. This is the primary onboarding document for contributors and AI agents.
