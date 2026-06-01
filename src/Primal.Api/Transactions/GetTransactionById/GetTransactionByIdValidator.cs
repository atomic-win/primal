using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;
using Primal.Domain.Money;

namespace Primal.Api.Transactions;

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
