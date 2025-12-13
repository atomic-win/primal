using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.AssetItems;

[HttpDelete("/api/assetItems/{id:guid}")]
internal sealed class DeleteAssetItemEndpoint : EndpointWithoutRequest
{
	private readonly IAssetItemRepository assetItemRepository;

	public DeleteAssetItemEndpoint(IAssetItemRepository assetItemRepository)
	{
		this.assetItemRepository = assetItemRepository;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var id = this.Route<Guid>("id");
		var assetItem = await this.assetItemRepository.GetByIdAsync(this.GetUserId(), id, ct);

		if (assetItem.Id == AssetItemId.Empty)
		{
			await this.Send.NotFoundAsync();
			return;
		}

		await this.assetItemRepository.DeleteAsync(this.GetUserId(), assetItem.Id, ct);
		await this.Send.NoContentAsync();
	}
}
