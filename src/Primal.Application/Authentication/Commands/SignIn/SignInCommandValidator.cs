using FluentValidation;

namespace Primal.Application.Authentication;

internal sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
	public SignInCommandValidator()
	{
		this.RuleFor(x => x.IdToken)
			.NotEmpty();
	}
}
