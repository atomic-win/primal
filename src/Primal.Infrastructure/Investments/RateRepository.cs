using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Dapper;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Investments;

internal sealed class RateRepository
{
	private static readonly int StalenessThresholdDays = 7;

	private readonly DbConnectionFactory connectionFactory;
	private readonly TimeProvider timeProvider;

	internal RateRepository(DbConnectionFactory connectionFactory, TimeProvider timeProvider)
	{
		this.connectionFactory = connectionFactory;
		this.timeProvider = timeProvider;
	}

	internal async Task<IReadOnlyDictionary<DateOnly, decimal>> GetRecentRatesAsync(
		string symbol,
		RateType rateType,
		CancellationToken cancellationToken)
	{
		var normalizedSymbol = symbol.ToUpperInvariant();
		var cutoffDate = DateOnly.FromDateTime(this.timeProvider.GetUtcNow().UtcDateTime)
			.AddDays(-StalenessThresholdDays);
		var parameters = new { Symbol = normalizedSymbol, RateType = rateType.ToString() };

		using var connection = this.connectionFactory.CreateConnection();

		var latestDate = await connection.QueryFirstOrDefaultAsync<string>(
			new CommandDefinition(
				"SELECT MAX(Date) FROM rates WHERE Symbol = @Symbol AND RateType = @RateType",
				parameters,
				cancellationToken: cancellationToken));

		if (latestDate is null
			|| DateOnly.ParseExact(latestDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) < cutoffDate)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var rows = await connection.QueryAsync<RateRow>(
			new CommandDefinition(
				"SELECT Date, Price FROM rates WHERE Symbol = @Symbol AND RateType = @RateType",
				parameters,
				cancellationToken: cancellationToken));

		return rows.ToFrozenDictionary(
			keySelector: row => DateOnly.ParseExact(row.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
			elementSelector: row => decimal.Parse(row.Price, CultureInfo.InvariantCulture));
	}

	internal async Task AddRatesAsync(
		string symbol,
		RateType rateType,
		IReadOnlyDictionary<DateOnly, decimal> rates,
		CancellationToken cancellationToken)
	{
		if (rates.Count == 0)
		{
			return;
		}

		var now = this.timeProvider.GetUtcNow().ToString("O");
		var normalizedSymbol = symbol.ToUpperInvariant();
		var rateTypeString = rateType.ToString();
		var parameters = new { Symbol = normalizedSymbol, RateType = rateTypeString };

		using var connection = this.connectionFactory.CreateConnection();

		var existingDates = await connection.QueryAsync<string>(
			new CommandDefinition(
				"SELECT Date FROM rates WHERE Symbol = @Symbol AND RateType = @RateType",
				parameters,
				cancellationToken: cancellationToken));

		var existingDateSet = existingDates.ToHashSet(StringComparer.Ordinal);

		var missingRates = rates
			.Where(r => !existingDateSet.Contains(r.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
			.ToList();

		if (missingRates.Count == 0)
		{
			return;
		}

		var valueClauses = new StringBuilder();
		var dynamicParameters = new DynamicParameters();

		for (int i = 0; i < missingRates.Count; i++)
		{
			if (i > 0)
			{
				valueClauses.Append(", ");
			}

			valueClauses.Append(CultureInfo.InvariantCulture, $"(@Symbol{i}, @RateType{i}, @Date{i}, @Price{i}, @CreatedAt{i})");
			dynamicParameters.Add($"Symbol{i}", normalizedSymbol);
			dynamicParameters.Add($"RateType{i}", rateTypeString);
			dynamicParameters.Add($"Date{i}", missingRates[i].Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			dynamicParameters.Add($"Price{i}", missingRates[i].Value.ToString(CultureInfo.InvariantCulture));
			dynamicParameters.Add($"CreatedAt{i}", now);
		}

		await connection.ExecuteAsync(
			new CommandDefinition(
				$"INSERT INTO rates (Symbol, RateType, Date, Price, CreatedAt) VALUES {valueClauses}",
				dynamicParameters,
				cancellationToken: cancellationToken));
	}

	private sealed record RateRow(string Date, string Price);
}
