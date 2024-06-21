using FluentValidation;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
{
	public AddTransactionCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.Date).GreaterThan(DateOnly.MinValue).LessThan(DateOnly.MaxValue);
		this.RuleFor(x => x.Name).NotEmpty();
		this.RuleFor(x => x.Type).IsInEnum().NotEqual(TransactionType.Unknown);
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
		this.RuleFor(x => x.Units).GreaterThan(0);
	}
}
