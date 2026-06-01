using System.Runtime.CompilerServices;
using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpGet("/api/asset-items/{assetItemId:guid}/transactions")]
internal sealed class GetAllByAssetItemIdEndpoint : Endpoint<GetAllByAssetItemIdRequest, IAsyncEnumerable<TransactionResponse>>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly IAssetItemRepository assetItemRepository;

	private readonly ITransactionAmountCalculator transactionAmountCalculator;

	public GetAllByAssetItemIdEndpoint(
		ITransactionRepository transactionRepository,
		IAssetItemRepository assetItemRepository,
		ITransactionAmountCalculator transactionAmountCalculator)
	{
		this.transactionRepository = transactionRepository;
		this.assetItemRepository = assetItemRepository;
		this.transactionAmountCalculator = transactionAmountCalculator;
	}

	public override async Task HandleAsync(
		GetAllByAssetItemIdRequest req,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.AssetItemId);

		var assetItem = await this.assetItemRepository.GetByIdAsync(
			userId,
			assetItemId,
			cancellationToken);

		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError(new FluentValidation.Results.ValidationFailure("assetItemId", "Asset item not found") { ErrorCode = "ASSET_ITEM_NOT_FOUND" }, StatusCodes.Status404NotFound);
		}

		var transactions = await this.transactionRepository.GetByAssetItemIdAsync(
			userId,
			assetItemId,
			cancellationToken);

		await this.Send.OkAsync(this.MapToResponses(userId, transactions, req.Currency, cancellationToken), cancellationToken);
	}

	private async IAsyncEnumerable<TransactionResponse> MapToResponses(
		UserId userId,
		IEnumerable<Transaction> transactions,
		Currency currency,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		foreach (var transaction in transactions)
		{
			yield return await transaction.ToResponse(
				userId,
				this.transactionAmountCalculator,
				currency,
				cancellationToken);
		}
	}
}
