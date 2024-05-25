using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
	public GetTransactionByIdQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.TransactionId.Value).NotEmpty();
	}
}
