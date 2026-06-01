using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpPatch("/api/asset-items/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class EditTransactionEndpoint : Endpoint<EditTransactionRequest>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly ITransactionRepository transactionRepository;

	public EditTransactionEndpoint(
		IAssetItemRepository assetItemRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(
		EditTransactionRequest req,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.AssetItemId);
		var transactionId = new TransactionId(req.TransactionId);

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, cancellationToken);
		if (assetItem.Id == AssetItemId.Empty)
		{
			this.AddError("Asset item not found", "ASSET_ITEM_NOT_FOUND");
			this.ThrowIfAnyErrors(StatusCodes.Status404NotFound);
		}

		var existingTransaction = await this.transactionRepository.GetByIdAsync(
			userId, assetItemId, transactionId, cancellationToken);
		if (existingTransaction.Id == TransactionId.Empty)
		{
			this.AddError("Transaction not found", "TRANSACTION_NOT_FOUND");
			this.ThrowIfAnyErrors(StatusCodes.Status404NotFound);
		}

		var normalized = this.NormalizeRequest(req, existingTransaction);

		await this.transactionRepository.UpdateAsync(
			userId,
			new Transaction(
				id: transactionId,
				normalized.Date,
				normalized.Name,
				normalized.TransactionType,
				assetItemId: assetItemId,
				units: normalized.Units,
				price: normalized.Price,
				amount: normalized.Amount),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}

	private NormalizedTransaction NormalizeRequest(
		EditTransactionRequest req,
		Transaction existingTransaction)
	{
		var name = string.IsNullOrWhiteSpace(req.Name) ? existingTransaction.Name : req.Name;
		var transactionType = req.TransactionType == TransactionType.Unknown
			? existingTransaction.TransactionType
			: req.TransactionType;

		if (!TransactionValidationExtensions.IsUnitsRequired(transactionType))
		{
			return new NormalizedTransaction(existingTransaction.Date, name, transactionType, 0, 0, req.Amount == 0 ? existingTransaction.Amount : req.Amount);
		}

		var units = req.Units == 0 ? existingTransaction.Units : req.Units;
		var price = req.Price == 0 ? existingTransaction.Price : req.Price;

		return new NormalizedTransaction(existingTransaction.Date, name, transactionType, units, price, 0);
	}

	private sealed record NormalizedTransaction(
		DateOnly Date,
		string Name,
		TransactionType TransactionType,
		decimal Units,
		decimal Price,
		decimal Amount);
}
