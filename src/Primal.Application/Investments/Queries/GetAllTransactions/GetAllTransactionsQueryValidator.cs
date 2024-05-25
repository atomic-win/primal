using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetAllTransactionsQueryValidator : AbstractValidator<GetAllTransactionsQuery>
{
	public GetAllTransactionsQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
	}
}
