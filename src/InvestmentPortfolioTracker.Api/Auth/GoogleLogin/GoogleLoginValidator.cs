using FastEndpoints;
using FluentValidation;
using InvestmentPortfolioTracker.Api.Errors;

namespace InvestmentPortfolioTracker.Api.Auth;

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
