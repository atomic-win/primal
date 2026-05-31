using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.AssetItems;

[HttpPost("/api/asset-items")]
internal sealed class AddAssetItemEndpoint : Endpoint<AddAssetItemRequest, AssetItemResponse>
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

	public override async Task HandleAsync(AddAssetItemRequest req, CancellationToken ct)
	{
		var asset = req.AssetType switch
		{
			AssetType.MutualFund => await this.GetOrCreateMutualFundAssetAsync(req, ct),
			AssetType.Stock => await this.GetOrCreateStockAssetAsync(req, ct),
			AssetType.Bond => await this.GetOrCreateDefaultAssetAsync(req with { AssetClass = AssetClass.Debt }, ct),
			_ => await this.GetOrCreateDefaultAssetAsync(req, ct),
		};

		var assetItem = await this.assetItemRepository.AddAsync(
			new UserId(req.UserId),
			asset.Id,
			req.Name,
			ct);

		var response = AssetItemResponse.From(assetItem, asset);

		await this.Send.CreatedAtAsync(
			$"/api/asset-items/{assetItem.Id.Value}",
			responseBody: response,
			cancellation: ct);
	}

	private async Task<Asset> GetOrCreateMutualFundAssetAsync(AddAssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"mf-{req.ExternalId}", ct);

		if (asset.Id != AssetId.Empty)
		{
			return asset;
		}

		var mutualFund = await this.mutualFundApiClient.GetBySymbolAsync(req.ExternalId, ct);

		if (string.IsNullOrWhiteSpace(mutualFund.SchemeCode))
		{
			this.ThrowError("Mutual fund not found", StatusCodes.Status404NotFound);
		}

		return await this.assetRepository.AddAsync(
			mutualFund.Name,
			req.AssetClass,
			AssetType.MutualFund,
			Currency.INR,
			$"mf-{mutualFund.SchemeCode}",
			ct);
	}

	private async Task<Asset> GetOrCreateStockAssetAsync(AddAssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"stock-{req.ExternalId.ToLowerInvariant()}", ct);

		if (asset.Id != AssetId.Empty)
		{
			return asset;
		}

		var stock = await this.stockApiClient.GetBySymbolAsync(req.ExternalId, ct);

		if (string.IsNullOrWhiteSpace(stock.Symbol))
		{
			this.ThrowError("Stock not found", StatusCodes.Status404NotFound);
		}

		return await this.assetRepository.AddAsync(
			stock.Name,
			AssetClass.Equity,
			AssetType.Stock,
			Currency.USD,
			$"stock-{stock.Symbol.ToLowerInvariant()}",
			ct);
	}

	private async Task<Asset> GetOrCreateDefaultAssetAsync(AddAssetItemRequest req, CancellationToken ct)
	{
		var externalId = $"default-{req.AssetClass}-{req.AssetType}-{req.Currency}";
		var asset = await this.assetRepository.GetByExternalIdAsync(externalId, ct);

		if (asset.Id != AssetId.Empty)
		{
			return asset;
		}

		return await this.assetRepository.AddAsync(
			externalId,
			req.AssetClass,
			req.AssetType,
			req.Currency,
			externalId,
			ct);
	}
}
