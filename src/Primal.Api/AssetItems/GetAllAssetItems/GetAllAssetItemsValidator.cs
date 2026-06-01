using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;

namespace Primal.Api.AssetItems;

internal sealed class GetAllAssetItemsValidator : Validator<GetAllAssetItemsRequest>
{
	public GetAllAssetItemsValidator()
	{
		this.RuleFor(x => x.UserId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.User.IdRequired)
			.WithErrorCode(ErrorCodes.User.IdRequired);
	}
}
