using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class StockApiClient : IAssetApiClient<Stock>
{
	private readonly string apiKey;
	private readonly IHttpClientFactory httpClientFactory;

	private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
	};

	internal StockApiClient(
		string apiKey,
		IHttpClientFactory httpClientFactory)
	{
		this.apiKey = apiKey;
		this.httpClientFactory = httpClientFactory;
	}

	public async Task<Stock> GetBySymbolAsync(string id, CancellationToken cancellationToken)
	{
		var httpClient = this.httpClientFactory.CreateClient(nameof(StockApiClient));
		var response = await httpClient.GetAsync($"/stable/search-symbol?query={id}&limit=1&apikey={this.apiKey}", cancellationToken);

		if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return new Stock(
				Symbol: string.Empty,
				Name: string.Empty,
				Currency: Currency.Unknown);
		}

		var apiResponse = await response.Content.ReadFromJsonAsync<IReadOnlyList<SymbolSearchApiResponse>>(this.jsonSerializerOptions, cancellationToken);

		if (apiResponse.Count == 0)
		{
			return new Stock(
				Symbol: string.Empty,
				Name: string.Empty,
				Currency: Currency.Unknown);
		}

		var symbolInfo = apiResponse[0];

		if (!Enum.TryParse<Currency>(symbolInfo.Currency, out var currency))
		{
			return new Stock(
				Symbol: string.Empty,
				Name: string.Empty,
				Currency: Currency.Unknown);
		}

		return new Stock(
			Symbol: symbolInfo.Symbol,
			Name: symbolInfo.Name,
			Currency: currency);
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(string symbol, CancellationToken cancellationToken)
	{
		var httpClient = this.httpClientFactory.CreateClient(nameof(StockApiClient));
		var response = await httpClient.GetAsync($"/stable/historical-price-eod/light?symbol={symbol}&from=2017-01-01&apikey={this.apiKey}", cancellationToken);

		if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var apiResponse = await response.Content.ReadFromJsonAsync<List<PriceResult>>(this.jsonSerializerOptions, cancellationToken);
		if (apiResponse.Count == 0)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		return apiResponse
			.ToFrozenDictionary(
				keySelector: result => DateOnly.ParseExact(result.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
				elementSelector: result => result.Price);
	}

	public Task<decimal> GetOnOrBeforePriceAsync(string id, DateOnly date, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	private sealed class SymbolSearchApiResponse
	{
		public string Symbol { get; set; }

		public string Name { get; set; }

		public string Currency { get; set; }
	}

	private sealed class PriceResult
	{
		public string Date { get; set; }

		public decimal Price { get; set; }
	}
}
