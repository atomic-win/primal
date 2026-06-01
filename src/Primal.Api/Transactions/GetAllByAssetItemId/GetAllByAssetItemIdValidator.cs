using FastEndpoints;
using FluentValidation;

namespace Primal.Api.Transactions;

internal sealed class GetAllByAssetItemIdValidator : Validator<GetAllByAssetItemIdRequest>
{
	public GetAllByAssetItemIdValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided");
	}
}
