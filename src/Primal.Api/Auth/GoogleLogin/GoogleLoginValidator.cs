using FastEndpoints;
using FluentValidation;

namespace Primal.Api.Auth;

internal sealed class GoogleLoginValidator : Validator<GoogleLoginRequest>
{
	public GoogleLoginValidator()
	{
		this.RuleFor(x => x.IdToken)
			.NotEmpty()
			.WithMessage("ID token must be provided");
	}
}
