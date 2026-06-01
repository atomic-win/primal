using FastEndpoints;
using FluentValidation;

namespace Primal.Api.Transactions;

internal sealed class GetTransactionByIdValidator : Validator<GetTransactionByIdRequest>
{
	public GetTransactionByIdValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided");

		this.RuleFor(x => x.TransactionId)
			.NotEqual(Guid.Empty)
			.WithMessage("Transaction ID must be provided");
	}
}
