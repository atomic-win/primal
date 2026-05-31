using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpPost("/api/asset-items/{assetItemId:guid}/transactions")]
internal sealed class AddTransactionEndpoint : Endpoint<AddTransactionRequest>
{
	private readonly ITransactionRepository transactionRepository;

	public AddTransactionEndpoint(
		ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(
		AddTransactionRequest req,
		CancellationToken cancellationToken)
	{
		var (units, price, amount) = this.NormalizeAmounts(req);

		var transaction = await this.transactionRepository.AddAsync(
			new UserId(req.UserId),
			new AssetItemId(req.AssetItemId),
			req.Date,
			req.Name,
			req.TransactionType,
			units,
			price,
			amount,
			cancellationToken);

		await this.Send.CreatedAtAsync(
			$"/api/asset-items/{req.AssetItemId}/transactions/{transaction.Id.Value}",
			cancellation: cancellationToken);
	}

	private (decimal Units, decimal Price, decimal Amount) NormalizeAmounts(AddTransactionRequest req)
	{
		if (!TransactionValidationExtensions.IsUnitsRequired(req.TransactionType))
		{
			return (0, 0, req.Amount);
		}

		return (req.Units, req.Price, 0);
	}
}
