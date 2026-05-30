using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.AssetItems;

[HttpGet("/api/asset-items/{id:guid}")]
internal sealed class GetAssetItemEndpoint : EndpointWithoutRequest<AssetItemResponse>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;

	public GetAssetItemEndpoint(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var userId = this.GetUserId();
		var assetItemId = new AssetItemId(this.Route<Guid>("id"));

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, ct);

		if (assetItem.Id == AssetItemId.Empty)
		{
			await this.Send.NotFoundAsync();
			return;
		}

		var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);

		await this.Send.OkAsync(
			new AssetItemResponse(
				assetItem.Id.Value,
				assetItem.Name,
				asset.AssetType,
				asset.AssetClass,
				asset.Currency),
			ct);
	}
}
