using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
	public DeleteTransactionCommandValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
		this.RuleFor(x => x.TransactionId.Value).NotEmpty();
	}
}
