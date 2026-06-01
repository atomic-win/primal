using FastEndpoints;
using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed class GetValuationsValidator : Validator<GetValuationsRequest>
{
	public GetValuationsValidator()
	{
		this.RuleFor(x => x.AssetItemIds)
			.NotEmpty()
			.WithMessage("At least one asset item ID must be provided")
			.WithErrorCode("ASSET_ITEM_IDS_REQUIRED");

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage("Currency must be provided")
			.WithErrorCode("CURRENCY_REQUIRED");
	}
}
