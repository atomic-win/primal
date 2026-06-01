using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;

namespace Primal.Api.AssetItems;

internal sealed class DeleteAssetItemValidator : Validator<DeleteAssetItemRequest>
{
	public DeleteAssetItemValidator()
	{
		this.RuleFor(x => x.Id)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.AssetItem.IdRequired)
			.WithErrorCode(ErrorCodes.AssetItem.IdRequired);
	}
}
