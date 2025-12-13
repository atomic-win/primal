using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Options;
using Primal.Application.Investments;
using Primal.Domain.Money;
using YahooFinanceApi;

namespace Primal.Infrastructure.Investments;

internal sealed class StockApiClient : IStockApiClient, IExchangeRateProvider
{
	public async Task<Stock> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		var securities = (await Yahoo.Symbols(symbol).QueryAsync(cancellationToken)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
		if (!securities.TryGetValue(symbol, out var security))
		{
			return new Stock(
				Symbol: string.Empty,
				Name: string.Empty,
				Currency: Currency.Unknown);
		}

		return new Stock(
			Symbol: security.Symbol,
			Name: security.LongName ?? string.Empty,
			Currency: Enum.TryParse<Currency>(security.Currency, out var currency) ? currency : Currency.Unknown);
	}

	public Task<IReadOnlyDictionary<DateOnly, decimal>> GetPriceAsync(string symbol, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(Currency from, Currency to, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}
}
