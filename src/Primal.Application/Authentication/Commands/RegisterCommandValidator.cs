using FluentValidation;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator()
	{
		this.RuleFor(x => x.IdToken)
			.NotEmpty();
	}
}
