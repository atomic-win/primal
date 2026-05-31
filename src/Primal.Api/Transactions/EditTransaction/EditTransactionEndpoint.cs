using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpPatch("/api/asset-items/{assetItemId:guid}/transactions/{transactionId:guid}")]
internal sealed class EditTransactionEndpoint : Endpoint<EditTransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;

	public EditTransactionEndpoint(
		ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(
		EditTransactionRequest req,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);
		var normalized = await this.NormalizeRequestAsync(userId, req, cancellationToken);

		await this.transactionRepository.UpdateAsync(
			userId,
			new Transaction(
				id: new TransactionId(req.TransactionId),
				normalized.Date,
				normalized.Name,
				normalized.TransactionType,
				assetItemId: new AssetItemId(req.AssetItemId),
				units: normalized.Units,
				price: normalized.Price,
				amount: normalized.Amount),
			cancellationToken);

		await this.Send.NoContentAsync(cancellation: cancellationToken);
	}

	private async Task<NormalizedTransaction> NormalizeRequestAsync(
		UserId userId,
		EditTransactionRequest req,
		CancellationToken cancellationToken)
	{
		var existingTransaction = await this.transactionRepository.GetByIdAsync(
			userId,
			new AssetItemId(req.AssetItemId),
			new TransactionId(req.TransactionId),
			cancellationToken);

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
