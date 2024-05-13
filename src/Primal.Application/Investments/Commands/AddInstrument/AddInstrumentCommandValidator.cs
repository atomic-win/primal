using FluentValidation;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddInstrumentCommandValidator : AbstractValidator<AddInstrumentCommand>
{
	public AddInstrumentCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Category).IsInEnum().NotEqual(InvestmentCategory.Unknown);

		this.RuleFor(x => x.Type).IsInEnum().NotEqual(InvestmentType.Unknown);
		this.RuleFor(x => x.Type).Must((x, type, context) =>
		{
			switch (x.Category)
			{
				case InvestmentCategory.BankAccount:
					return type == InvestmentType.SalaryAccount || type == InvestmentType.SavingsAccount;
				case InvestmentCategory.Deposits:
					return type == InvestmentType.FixedDeposit || type == InvestmentType.RecurringDeposit || type == InvestmentType.TermDeposit;
				case InvestmentCategory.PF:
					return type == InvestmentType.PPF || type == InvestmentType.EPF;
				case InvestmentCategory.Equity:
					return type == InvestmentType.Stocks || type == InvestmentType.MutualFunds;
				default:
					return false;
			}
		}).WithMessage(x => $"The investment type '{x.Type}' is not valid for the investment category '{x.Category}'.");

		this.RuleFor(x => x.AccountId.Value).NotNull();
	}
}
