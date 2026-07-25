using FastEndpoints;
using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Core.Investments;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.AssetItems;

[HttpPatch("/api/asset-items/{id:guid}")]
internal sealed class EditAssetItemEndpoint : Endpoint<EditAssetItemRequest>
{
	private readonly IAssetItemRepository assetItemRepository;

	public EditAssetItemEndpoint(IAssetItemRepository assetItemRepository)
	{
		this.assetItemRepository = assetItemRepository;
	}

	public override async Task HandleAsync(EditAssetItemRequest req, CancellationToken ct)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.Id);

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, ct);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError(ErrorFactory.AssetItemNotFound(), StatusCodes.Status404NotFound);
		}

		if (string.Equals(assetItem.Name, req.Name, StringComparison.Ordinal))
		{
			await this.Send.NoContentAsync(cancellation: ct);
			return;
		}

		var updatedAssetItem = new AssetItem(assetItemId, assetItem.AssetId, req.Name);

		await this.assetItemRepository.UpdateAsync(userId, updatedAssetItem, ct);

		await this.Send.NoContentAsync(cancellation: ct);
	}
}
