using FastEndpoints;
using Primal.Api.Errors;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.AssetItems;

[HttpGet("/api/asset-items/{id:guid}")]
internal sealed class GetAssetItemEndpoint : Endpoint<GetAssetItemRequest, AssetItemResponse>
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

	public override async Task HandleAsync(GetAssetItemRequest req, CancellationToken ct)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.Id);

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, ct);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError(ErrorFactory.AssetItemNotFound(), StatusCodes.Status404NotFound);
		}

		var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);

		await this.Send.OkAsync(AssetItemResponse.From(assetItem, asset), ct);
	}
}
