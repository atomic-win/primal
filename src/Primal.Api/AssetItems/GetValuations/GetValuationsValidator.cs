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
			.WithMessage("At least one asset item ID must be provided");

		this.RuleFor(x => x.Currency)
			.NotEqual(Currency.Unknown)
			.WithMessage("Currency must be provided");
	}
}
