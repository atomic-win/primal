using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class ExchangeRateApiClient : IExchangeRateApiClient
{
	private readonly string apiKey;
	private readonly IHttpClientFactory httpClientFactory;

	public ExchangeRateApiClient(
		string apiKey,
		IHttpClientFactory httpClientFactory)
	{
		this.apiKey = apiKey;
		this.httpClientFactory = httpClientFactory;
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(Currency from, Currency to, CancellationToken cancellationToken)
	{
		if (from == to)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var requestUri = $"/query?&apikey={this.apiKey}&datatype=csv&function=FX_DAILY&from_symbol={from}&to_symbol={to}&outputsize=full";

		using (var reader = new StreamReader(await this.httpClientFactory.CreateClient(nameof(ExchangeRateApiClient)).GetStreamAsync(requestUri, cancellationToken)))
		{
			var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

			return csvReader.GetRecords<ExchangeRate>()
				.ToFrozenDictionary(
					keySelector: x => DateOnly.Parse(x.Date, CultureInfo.InvariantCulture),
					elementSelector: x => x.Close);
		}
	}

	public Task<decimal> GetOnOrBeforeExchangeRateAsync(Currency from, Currency to, DateOnly date, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	private sealed class ExchangeRate
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
	}
}
