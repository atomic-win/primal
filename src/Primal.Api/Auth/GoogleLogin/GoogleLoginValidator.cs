using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;

namespace Primal.Api.Auth;

internal sealed class GoogleLoginValidator : Validator<GoogleLoginRequest>
{
	public GoogleLoginValidator()
	{
		this.RuleFor(x => x.IdToken)
			.NotEmpty()
			.WithMessage(ErrorMessages.Auth.IdTokenRequired)
			.WithErrorCode(ErrorCodes.Auth.IdTokenRequired);
	}
}
