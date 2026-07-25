using FastEndpoints;
using FluentValidation;
using InvestmentPortfolioTracker.Api.Errors;

namespace InvestmentPortfolioTracker.Api.Users;

internal sealed class GetUserValidator : Validator<GetUserRequest>
{
	public GetUserValidator()
	{
		this.RuleFor(x => x.UserId)
			.NotEqual(Guid.Empty)
			.WithMessage(ErrorMessages.User.IdRequired)
			.WithErrorCode(ErrorCodes.User.IdRequired);
	}
}
