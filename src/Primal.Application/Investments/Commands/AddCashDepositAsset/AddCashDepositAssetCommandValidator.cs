using FluentValidation;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddCashDepositAssetCommandValidator : AbstractValidator<AddCashDepositAssetCommand>
{
	public AddCashDepositAssetCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Type).IsInEnum().NotEqual(InstrumentType.Unknown).NotEqual(InstrumentType.MutualFunds).NotEqual(InstrumentType.Stocks);
	}
}
