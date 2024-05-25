using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class AddCashDepositAssetCommandValidator : AbstractValidator<AddCashDepositAssetCommand>
{
	public AddCashDepositAssetCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
	}
}
