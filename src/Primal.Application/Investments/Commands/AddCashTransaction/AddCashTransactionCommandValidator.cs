using FluentValidation;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class AddCashTransactionCommandValidator : AbstractValidator<AddCashTransactionCommand>
{
	public AddCashTransactionCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Date).GreaterThan(DateOnly.MinValue).LessThan(DateOnly.MaxValue);
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Type).IsInEnum().Must(x => x != TransactionType.Unknown && x != TransactionType.Buy && x != TransactionType.Sell);
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
		this.RuleFor(x => x.Amount).GreaterThan(0);
		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
	}
}
