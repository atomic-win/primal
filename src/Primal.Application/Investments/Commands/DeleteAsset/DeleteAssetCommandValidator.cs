using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class DeleteAssetCommandValidator : AbstractValidator<DeleteAssetCommand>
{
	public DeleteAssetCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
	}
}
