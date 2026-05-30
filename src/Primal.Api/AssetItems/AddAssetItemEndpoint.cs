using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

[HttpPost("/api/asset-items")]
internal sealed class AddAssetItemEndpoint : Endpoint<AssetItemRequest>
{
	private readonly IAssetApiClient<MutualFund> mutualFundApiClient;
	private readonly IAssetApiClient<Stock> stockApiClient;

	private readonly IAssetRepository assetRepository;
	private readonly IAssetItemRepository assetItemRepository;

	public AddAssetItemEndpoint(
		IAssetApiClient<MutualFund> mutualFundApiClient,
		IAssetApiClient<Stock> stockApiClient,
		IAssetRepository assetRepository,
		IAssetItemRepository assetItemRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.assetRepository = assetRepository;
		this.assetItemRepository = assetItemRepository;
	}

	public override async Task HandleAsync(AssetItemRequest req, CancellationToken ct)
	{
		if (req.AssetType == AssetType.MutualFund)
		{
			await this.AddMutualFundAsync(req, ct);
			return;
		}

		if (req.AssetType == AssetType.Stock)
		{
			await this.AddStockAsync(req, ct);
			return;
		}

		if (req.AssetType == AssetType.Bond)
		{
			await this.AddOtherAssetItemTypeAsync(req with { AssetClass = AssetClass.Debt }, ct);
			return;
		}

		await this.AddOtherAssetItemTypeAsync(req, ct);
	}

	private async Task AddMutualFundAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"mf-{req.ExternalId}", ct);

		if (asset.Id == AssetId.Empty)
		{
			var mutualFund = await this.mutualFundApiClient.GetBySymbolAsync(req.ExternalId, ct);

			if (string.IsNullOrWhiteSpace(mutualFund.SchemeCode))
			{
				this.ThrowError("Mutual fund not found", StatusCodes.Status404NotFound);
			}

			asset = await this.assetRepository.AddAsync(
				mutualFund.Name,
				req.AssetClass,
				AssetType.MutualFund,
				Currency.INR,
				$"mf-{mutualFund.SchemeCode}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddStockAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"stock-{req.ExternalId.ToLowerInvariant()}", ct);
		if (asset.Id == AssetId.Empty)
		{
			var stock = await this.stockApiClient.GetBySymbolAsync(req.ExternalId, ct);

			if (string.IsNullOrWhiteSpace(stock.Symbol))
			{
				this.ThrowError("Stock not found", StatusCodes.Status404NotFound);
			}

			asset = await this.assetRepository.AddAsync(
				stock.Name,
				AssetClass.Equity,
				AssetType.Stock,
				Currency.USD,
				$"stock-{stock.Symbol.ToLowerInvariant()}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddOtherAssetItemTypeAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"default-{req.AssetClass}-{req.AssetType}-{req.Currency}", ct);

		if (asset.Id == AssetId.Empty)
		{
			asset = await this.assetRepository.AddAsync(
				$"default-{req.AssetClass}-{req.AssetType}-{req.Currency}",
				req.AssetClass,
				req.AssetType,
				req.Currency,
				$"default-{req.AssetClass}-{req.AssetType}-{req.Currency}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddAssetItemAsync(
		AssetId assetId,
		string name,
		CancellationToken ct)
	{
		await this.assetItemRepository.AddAsync(
			this.GetUserId(),
			assetId,
			name,
			ct);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "used only in this file")]
internal sealed record AssetItemRequest(
	string Name,
	AssetClass AssetClass,
	AssetType AssetType,
	string ExternalId,
	Currency Currency);
