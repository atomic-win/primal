using FastEndpoints;
using FluentValidation;

using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Users;

internal sealed class UpdateUserValidator : Validator<UpdateUserRequest>
{
	public UpdateUserValidator()
	{
		this.RuleFor(x => x)
			.Must(req => req.PreferredCurrency != Currency.Unknown || req.PreferredLocale != Locale.Unknown)
			.WithMessage(ErrorMessages.User.UpdateFieldsRequired)
			.WithErrorCode(ErrorCodes.User.UpdateFieldsRequired);
	}
}
