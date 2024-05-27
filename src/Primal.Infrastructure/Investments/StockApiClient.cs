using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using ErrorOr;
using Microsoft.Extensions.Options;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class StockApiClient : IStockApiClient
{
	private readonly InvestmentSettings investmentSettings;
	private readonly HttpClient httpClient;

	public StockApiClient(IOptions<InvestmentSettings> investmentSettings, HttpClient httpClient)
	{
		this.investmentSettings = investmentSettings.Value;
		this.httpClient = httpClient;
	}

	public async Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		var requestUri = $"query?&apikey={this.investmentSettings.AlphaVantageApiKey}&datatype=csv&function=SYMBOL_SEARCH&keywords={symbol}";

		using (var reader = new StreamReader(await this.httpClient.GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csvReader.GetRecords<SymbolSearch>().ToList();

			var symbolSearch = records.OrderByDescending(x => x.MatchScore).FirstOrDefault();

			if (symbolSearch == null)
			{
				return Error.NotFound();
			}

			if (!Enum.TryParse<Currency>(symbolSearch.Currency, out var currency))
			{
				return Error.Validation($"Invalid currency '{symbolSearch.Currency}'");
			}

			return new Stock(
				new InstrumentId(Guid.Empty),
				symbolSearch.Name,
				symbolSearch.Symbol,
				symbolSearch.Type,
				symbolSearch.Region,
				symbolSearch.MarketOpen,
				symbolSearch.MarketClose,
				symbolSearch.Timezone,
				currency);
		}
	}

	public async Task<ErrorOr<IEnumerable<InstrumentValue>>> GetHistoricalValuesAsync(string symbol, CancellationToken cancellationToken)
	{
		var requestUri = $"query?&apikey={this.investmentSettings.AlphaVantageApiKey}&datatype=csv&function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full";

		using (var reader = new StreamReader(await this.httpClient.GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
			var records = csvReader.GetRecords<HistoricalValue>().ToList();

			return records
				.Select(x => new InstrumentValue(
					DateOnly.Parse(x.Date, CultureInfo.InvariantCulture),
					x.Close))
				.ToArray();
		}
	}

	private sealed class SymbolSearch
	{
		[Name("symbol")]
		public string Symbol { get; init; }

		[Name("name")]
		public string Name { get; init; }

		[Name("type")]
		public string Type { get; init; }

		[Name("region")]
		public string Region { get; init; }

		[Name("marketOpen")]
		public string MarketOpen { get; init; }

		[Name("marketClose")]
		public string MarketClose { get; init; }

		[Name("timezone")]
		public string Timezone { get; init; }

		[Name("currency")]
		public string Currency { get; init; }

		[Name("matchScore")]
		public double MatchScore { get; init; }
	}

	private sealed class HistoricalValue
	{
		[Name("timestamp")]
		public string Date { get; init; }

		[Name("open")]
		public decimal Open { get; init; }

		[Name("high")]
		public decimal High { get; init; }

		[Name("low")]
		public decimal Low { get; init; }

		[Name("close")]
		public decimal Close { get; init; }

		[Name("volume")]
		public long Volume { get; init; }
	}
}
