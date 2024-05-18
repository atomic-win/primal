using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class AddStockAssetCommandValidator : AbstractValidator<AddStockAssetCommand>
{
	public AddStockAssetCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Symbol).NotEmpty();
	}
}
