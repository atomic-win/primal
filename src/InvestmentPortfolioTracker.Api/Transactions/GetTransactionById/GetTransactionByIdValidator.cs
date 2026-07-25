using FastEndpoints;
using FluentValidation;
using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.Transactions;

internal sealed class GetTransactionByIdValidator : Validator<GetTransactionByIdRequest>
{
	public GetTransactionByIdValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);

		this.RuleFor(x => x.TransactionId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.Transaction.IdRequired)
			.WithErrorCode(ErrorCodes.Transaction.IdRequired);

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage(ErrorMessages.AssetItem.CurrencyRequired)
			.WithErrorCode(ErrorCodes.AssetItem.CurrencyRequired);
	}
}
