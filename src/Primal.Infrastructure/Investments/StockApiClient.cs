using System.Collections.Frozen;
using Primal.Application.Investments;
using Primal.Domain.Money;
using YahooFinanceApi;

namespace Primal.Infrastructure.Investments;

internal sealed class StockApiClient : IStockApiClient, IExchangeRateProvider
{
	public async Task<Stock> GetByIdAsync(string id, CancellationToken cancellationToken)
	{
		var securities = (await Yahoo.Symbols(id).QueryAsync(cancellationToken)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
		if (!securities.TryGetValue(id, out var security))
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

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPricesAsync(string id, CancellationToken cancellationToken)
	{
		return await this.GetHistoricalAsync(id, cancellationToken);
	}

	public Task<decimal> GetOnOrBeforePriceAsync(string id, DateOnly date, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetExchangeRatesAsync(Currency from, Currency to, CancellationToken cancellationToken)
	{
		return await this.GetHistoricalAsync($"{from}{to}=X", cancellationToken);
	}

	public Task<decimal> GetOnOrBeforeExchangeRateAsync(Currency from, Currency to, DateOnly date, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	private async Task<IReadOnlyDictionary<DateOnly, decimal>> GetHistoricalAsync(string symbol, CancellationToken cancellationToken)
	{
		var candles = await Yahoo.GetHistoricalAsync(symbol, token: cancellationToken);

		return candles.ToFrozenDictionary(
			keySelector: candle => DateOnly.FromDateTime(candle.DateTime),
			elementSelector: candle => candle.Close);
	}
}
