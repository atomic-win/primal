using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed class GetValuationsValidator : Validator<GetValuationsRequest>
{
	public GetValuationsValidator()
	{
		this.RuleFor(x => x.AssetItemIds)
			.NotEmpty()
			.WithMessage(ErrorMessages.AssetItem.IdsRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdsRequired);

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage(ErrorMessages.AssetItem.CurrencyRequired)
			.WithErrorCode(ErrorCodes.AssetItem.CurrencyRequired);
	}
}
