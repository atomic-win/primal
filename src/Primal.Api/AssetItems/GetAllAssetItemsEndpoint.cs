using System.Runtime.CompilerServices;
using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

[HttpGet("/api/assetItems")]
internal sealed class GetAllAssetItemsEndpoint : EndpointWithoutRequest<IAsyncEnumerable<AssetItemResponse>>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;
	private readonly ITransactionRepository transactionRepository;

	public GetAllAssetItemsEndpoint(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
		this.transactionRepository = transactionRepository;
	}

	public override async Task<IAsyncEnumerable<AssetItemResponse>> ExecuteAsync(CancellationToken ct)
	{
		var assetItems = await this.assetItemRepository.GetAllAsync(this.GetUserId(), ct);

		return this.MapToResponsesAsync(assetItems, ct);
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

		var earliestDate = await this.transactionRepository.GetEarliestTransactionDateAsync(this.GetUserId(), assetItem.Id, ct);

		return new AssetItemResponse(
			assetItem.Id.Value,
			assetItem.Name,
			asset.AssetType,
			asset.AssetClass,
			asset.Currency,
			ActivityStartDate: earliestDate == default ? DateOnly.FromDateTime(DateTime.Today) : earliestDate);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "used only in this file")]
internal sealed record AssetItemResponse(
	Guid Id,
	string Name,
	AssetType AssetType,
	AssetClass AssetClass,
	Currency Currency,
	DateOnly ActivityStartDate);
