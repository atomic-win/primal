using FastEndpoints;
using FluentValidation;

namespace Primal.Api.AssetItems;

internal sealed class DeleteAssetItemValidator : Validator<DeleteAssetItemRequest>
{
	public DeleteAssetItemValidator()
	{
		this.RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided");
	}
}
