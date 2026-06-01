using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.AssetItems;

[HttpDelete("/api/asset-items/{id:guid}")]
internal sealed class DeleteAssetItemEndpoint : Endpoint<DeleteAssetItemRequest>
{
	private readonly IAssetItemRepository assetItemRepository;

	public DeleteAssetItemEndpoint(IAssetItemRepository assetItemRepository)
	{
		this.assetItemRepository = assetItemRepository;
	}

	public override async Task HandleAsync(DeleteAssetItemRequest req, CancellationToken ct)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.Id);

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, ct);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError("Asset item not found", "ASSET_ITEM_NOT_FOUND", statusCode: StatusCodes.Status404NotFound);
		}

		await this.assetItemRepository.DeleteAsync(userId, assetItem.Id, ct);
		await this.Send.NoContentAsync();
	}
}
