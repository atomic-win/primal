using FluentValidation;

namespace Primal.Application.Users;

internal sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
	public GetUserQueryValidator()
	{
		this.RuleFor(x => x.UserId).NotEmpty();
	}
}
