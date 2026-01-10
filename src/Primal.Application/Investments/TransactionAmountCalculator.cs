using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed class TransactionAmountCalculator
{
	private readonly IAssetApiClient<MutualFund> mutualFundApiClient;
	private readonly IAssetApiClient<Stock> stockApiClient;
	private readonly IExchangeRateApiClient exchangeRateProvider;

	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public TransactionAmountCalculator(
		IAssetApiClient<MutualFund> mutualFundApiClient,
		IAssetApiClient<Stock> stockApiClient,
		IExchangeRateApiClient exchangeRateProvider,
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.exchangeRateProvider = exchangeRateProvider;
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<decimal> CalculateAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly date,
		Currency targetCurrency,
		CancellationToken cancellationToken)
	{
		var asset = await this.GetAssetAsync(
			userId,
			transaction.AssetItemId,
			cancellationToken);

		var exchangeRate = await this.exchangeRateProvider.GetOnOrBeforeExchangeRateAsync(
			asset.Currency,
			targetCurrency,
			date,
			cancellationToken);

		return exchangeRate * (await this.GetAmountAsync(
			asset,
			transaction,
			date,
			cancellationToken));
	}

	private async Task<Asset> GetAssetAsync(
		UserId userId,
		AssetItemId assetItemId,
		CancellationToken cancellationToken)
	{
		var assetItem = await this.assetItemRepository.GetByIdAsync(
			userId,
			assetItemId,
			cancellationToken);

		return await this.assetRepository.GetByIdAsync(
			assetItem.AssetId,
			cancellationToken);
	}

	private async Task<decimal> GetAmountAsync(
		Asset asset,
		Transaction transaction,
		DateOnly date,
		CancellationToken cancellationToken)
	{
		if (transaction.TransactionType != TransactionType.Buy
			&& transaction.TransactionType != TransactionType.Sell)
		{
			return transaction.Amount;
		}

		if (date == transaction.Date)
		{
			return transaction.Units * transaction.Price;
		}

		var assetRate = await this.GetAssetRateAsync(
			asset,
			date,
			cancellationToken);

		return transaction.Units * assetRate;
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

		var symbol = asset.ExternalId.Split('-')[1];

		if (asset.AssetType == AssetType.MutualFund)
		{
			return await this.mutualFundApiClient.GetOnOrBeforePriceAsync(
				symbol,
				date,
				cancellationToken);
		}

		return await this.stockApiClient.GetOnOrBeforePriceAsync(
			symbol,
			date,
			cancellationToken);
	}
}
