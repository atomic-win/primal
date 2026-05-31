using FastEndpoints;
using FluentValidation;

namespace Primal.Api.AssetItems;

internal sealed class GetAssetItemValidator : Validator<GetAssetItemRequest>
{
	public GetAssetItemValidator()
	{
		this.RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided.");
	}
}
