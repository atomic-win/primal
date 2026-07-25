using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed class DeleteTransactionValidator : Validator<DeleteTransactionRequest>
{
	public DeleteTransactionValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);

		this.RuleFor(x => x.TransactionId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.Transaction.IdRequired)
			.WithErrorCode(ErrorCodes.Transaction.IdRequired);
	}
}
