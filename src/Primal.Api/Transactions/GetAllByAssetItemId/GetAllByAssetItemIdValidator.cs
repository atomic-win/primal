using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;

namespace Primal.Api.Transactions;

internal sealed class GetAllByAssetItemIdValidator : Validator<GetAllByAssetItemIdRequest>
{
	public GetAllByAssetItemIdValidator()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);
	}
}
