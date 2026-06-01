using FastEndpoints;
using FluentValidation;

namespace Primal.Api.AssetItems;

internal sealed class GetAllAssetItemsValidator : Validator<GetAllAssetItemsRequest>
{
	public GetAllAssetItemsValidator()
	{
		this.RuleFor(x => x.UserId)
			.NotEqual(Guid.Empty)
			.WithMessage("User ID must be provided")
			.WithErrorCode("USER_ID_REQUIRED");
	}
}
