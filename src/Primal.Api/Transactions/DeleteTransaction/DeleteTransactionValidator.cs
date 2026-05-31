using FastEndpoints;
using FluentValidation;

namespace Primal.Api.Transactions;

internal sealed class DeleteTransactionValidator : Validator<DeleteTransactionRequest>
{
	public DeleteTransactionValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided.");

		this.RuleFor(x => x.TransactionId)
			.NotEqual(Guid.Empty)
			.WithMessage("Transaction ID must be provided.");
	}
}
