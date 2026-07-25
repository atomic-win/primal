using System.Runtime.CompilerServices;

using FastEndpoints;

using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.AssetItems;

[HttpGet("/api/asset-items")]
internal sealed class GetAllAssetItemsEndpoint : Endpoint<GetAllAssetItemsRequest, IAsyncEnumerable<AssetItemResponse>>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public GetAllAssetItemsEndpoint(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
	}

	public override async Task HandleAsync(GetAllAssetItemsRequest req, CancellationToken ct)
	{
		var userId = new UserId(req.UserId);
		var assetItems = await this.assetItemRepository.GetAllAsync(userId, ct);

		await this.Send.OkAsync(this.MapToResponsesAsync(assetItems, ct), ct);
	}

	private async IAsyncEnumerable<AssetItemResponse> MapToResponsesAsync(
		IEnumerable<AssetItem> assetItems,
		[EnumeratorCancellation] CancellationToken ct)
	{
		foreach (var assetItem in assetItems)
		{
			yield return await this.MapToResponseAsync(assetItem, ct);
		}
	}

	private async Task<AssetItemResponse> MapToResponseAsync(
		AssetItem assetItem,
		CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);

		return AssetItemResponse.From(assetItem, asset);
	}
}
