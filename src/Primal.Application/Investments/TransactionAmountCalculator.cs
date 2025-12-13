using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed class TransactionAmountCalculator
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;
	private readonly IExchangeRateProvider exchangeRateProvider;

	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public TransactionAmountCalculator(
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient,
		IExchangeRateProvider exchangeRateProvider,
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.exchangeRateProvider = exchangeRateProvider;
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<decimal> CalculateAmount(
		UserId userId,
		Transaction transaction,
		DateOnly date,
		Currency targetCurrency,
		CancellationToken cancellationToken)
	{
		var assetItem = await this.assetItemRepository.GetByIdAsync(
			userId,
			transaction.AssetItemId,
			cancellationToken);

		var asset = await this.assetRepository.GetByIdAsync(
			assetItem.AssetId,
			cancellationToken);

		var exchangeRate = await this.GetExchangeRateAsync(
			asset.Currency,
			targetCurrency,
			date,
			cancellationToken);

		if (transaction.TransactionType == TransactionType.Dividend)
		{
			return transaction.Units * exchangeRate;
		}

		var assetRate = await this.GetAssetRateAsync(
			asset,
			date,
			cancellationToken);

		return transaction.Units * assetRate * exchangeRate;
	}

	private async Task<decimal> GetExchangeRateAsync(
		Currency from,
		Currency to,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var rates = await this.exchangeRateProvider.GetExchangeRatesAsync(
			from,
			to,
			cancellationToken);

		return this.GetOnOrBeforeValue(rates, date);
	}

	private async Task<decimal> GetAssetRateAsync(
		Asset asset,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		if (asset.AssetType != AssetType.MutualFund && asset.AssetType != AssetType.Stock)
		{
			return 1m;
		}

		return asset.AssetType == AssetType.MutualFund
			? await this.GetMutualFundRateAsync(asset.ExternalId, date, cancellationToken)
			: await this.GetStockRateAsync(asset.ExternalId, date, cancellationToken);
	}

	private async Task<decimal> GetMutualFundRateAsync(
		string externalId,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var rates = await this.mutualFundApiClient.GetPricesAsync(
			externalId,
			cancellationToken);

		return this.GetOnOrBeforeValue(rates, date);
	}

	private async Task<decimal> GetStockRateAsync(
		string externalId,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		var rates = await this.stockApiClient.GetPricesAsync(
			externalId,
			cancellationToken);

		return this.GetOnOrBeforeValue(rates, date);
	}

	private decimal GetOnOrBeforeValue(
		IReadOnlyDictionary<DateOnly, decimal> rates,
		DateOnly date)
	{
		for (int lookback = 0; lookback < 7; ++lookback)
		{
			if (rates.TryGetValue(date.AddDays(-lookback), out var rate))
			{
				return rate;
			}
		}

		throw new InvalidOperationException(
			$"No rate found for date {date} or within the lookback period.");
	}
}
