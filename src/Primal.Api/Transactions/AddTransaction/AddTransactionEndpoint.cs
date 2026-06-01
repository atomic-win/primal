using FastEndpoints;
using Primal.Api.Errors;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

[HttpPost("/api/asset-items/{assetItemId:guid}/transactions")]
internal sealed class AddTransactionEndpoint : Endpoint<AddTransactionRequest, TransactionResponse>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly ITransactionRepository transactionRepository;

	public AddTransactionEndpoint(
		IAssetItemRepository assetItemRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.transactionRepository = transactionRepository;
	}

	public override async Task HandleAsync(
		AddTransactionRequest req,
		CancellationToken cancellationToken)
	{
		var userId = new UserId(req.UserId);
		var assetItemId = new AssetItemId(req.AssetItemId);

		var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, cancellationToken);
		if (assetItem.Id == AssetItemId.Empty)
		{
			this.ThrowError(ErrorFactory.AssetItemNotFound("assetItemId"), StatusCodes.Status404NotFound);
		}

		var (units, price, amount) = this.NormalizeAmounts(req);

		var transaction = await this.transactionRepository.AddAsync(
			userId,
			assetItemId,
			req.Date,
			req.Name,
			req.TransactionType,
			units,
			price,
			amount,
			cancellationToken);

		var response = new TransactionResponse(
			transaction.Id.Value,
			transaction.Date,
			transaction.Name,
			transaction.TransactionType,
			transaction.AssetItemId.Value,
			0,
			0,
			0);

		await this.Send.CreatedAtAsync(
			$"/api/asset-items/{req.AssetItemId}/transactions/{transaction.Id.Value}",
			responseBody: response,
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
