using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class AddMutualFundAssetCommandValidator : AbstractValidator<AddMutualFundAssetCommand>
{
	public AddMutualFundAssetCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.SchemeCode).NotEmpty();
	}
}
