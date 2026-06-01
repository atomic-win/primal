using FastEndpoints;
using FluentValidation;
using Primal.Api.Errors;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

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
