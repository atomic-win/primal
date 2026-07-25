using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Infrastructure.Investments;

internal sealed class AlphaVantageApiClient : IAssetApiClient<Stock>, IForexApiClient
{
	private readonly string apiKey;
	private readonly IHttpClientFactory httpClientFactory;

	internal AlphaVantageApiClient(
		string apiKey,
		IHttpClientFactory httpClientFactory)
	{
		this.apiKey = apiKey;
		this.httpClientFactory = httpClientFactory;
	}

	public async Task<Stock> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		var encodedSymbol = Uri.EscapeDataString(symbol);
		var encodedApiKey = Uri.EscapeDataString(this.apiKey);
		var requestUri = $"/query?apikey={encodedApiKey}&datatype=csv&function=SYMBOL_SEARCH&keywords={encodedSymbol}";

		using (var reader = new StreamReader(
			await this.CreateClient().GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
			var results = csvReader.GetRecords<SymbolSearchResult>().ToList();

			if (results.Count == 0)
			{
				return new Stock(
					Symbol: string.Empty,
					Name: string.Empty,
					AssetType: AssetType.Unknown,
					Currency: Currency.Unknown);
			}

			var match = results[0];

			if (!Enum.TryParse<Currency>(match.Currency, out var currency))
			{
				return new Stock(
					Symbol: string.Empty,
					Name: string.Empty,
					AssetType: AssetType.Unknown,
					Currency: Currency.Unknown);
			}

			var assetType = match.Type switch
			{
				"Equity" => AssetType.Stock,
				"ETF" => AssetType.ETF,
				_ => throw new NotSupportedException(
					$"Unsupported symbol type '{match.Type}' for symbol '{match.Symbol}'."),
			};

			return new Stock(
				Symbol: match.Symbol,
				Name: match.Name,
				AssetType: assetType,
				Currency: currency);
		}
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(
		string symbol,
		CancellationToken cancellationToken)
	{
		var encodedSymbol = Uri.EscapeDataString(symbol);
		var encodedApiKey = Uri.EscapeDataString(this.apiKey);
		var requestUri = $"/query?apikey={encodedApiKey}&datatype=csv&function=TIME_SERIES_DAILY&symbol={encodedSymbol}";

		using (var reader = new StreamReader(
			await this.CreateClient().GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
			var results = csvReader.GetRecords<TimeSeriesResult>().ToList();

			if (results.Count == 0)
			{
				return ImmutableDictionary<DateOnly, decimal>.Empty;
			}

			return results.ToFrozenDictionary(
				keySelector: r => DateOnly.Parse(r.Timestamp, CultureInfo.InvariantCulture),
				elementSelector: r => r.Close);
		}
	}

	public Task<decimal> GetOnOrBeforePriceAsync(string symbol, DateOnly date, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetForexRatesAsync(
		Currency from,
		Currency to,
		CancellationToken cancellationToken)
	{
		if (from == to)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var encodedApiKey = Uri.EscapeDataString(this.apiKey);
		var requestUri = $"/query?apikey={encodedApiKey}&datatype=csv&function=FX_DAILY&from_symbol={from}&to_symbol={to}&outputsize=full";

		using (var reader = new StreamReader(
			await this.CreateClient().GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

			return csvReader.GetRecords<TimeSeriesResult>()
				.ToFrozenDictionary(
					keySelector: x => DateOnly.Parse(x.Timestamp, CultureInfo.InvariantCulture),
					elementSelector: x => x.Close);
		}
	}

	public Task<decimal> GetOnOrBeforeForexRateAsync(
		Currency from,
		Currency to,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	private HttpClient CreateClient()
	{
		return this.httpClientFactory.CreateClient(nameof(AlphaVantageApiClient));
	}

	private sealed class SymbolSearchResult
	{
		[Name("symbol")]
		public string Symbol { get; init; }

		[Name("name")]
		public string Name { get; init; }

		[Name("type")]
		public string Type { get; init; }

		[Name("currency")]
		public string Currency { get; init; }
	}

	private sealed class TimeSeriesResult
	{
		[Name("timestamp")]
		public string Timestamp { get; init; }

		[Name("close")]
		public decimal Close { get; init; }
	}
}
