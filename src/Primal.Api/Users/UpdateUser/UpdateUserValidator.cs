using FastEndpoints;
using FluentValidation;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

internal sealed class UpdateUserValidator : Validator<UpdateUserRequest>
{
	public UpdateUserValidator()
	{
		this.RuleFor(x => x)
			.Must(req => req.PreferredCurrency != Currency.Unknown || req.PreferredLocale != Locale.Unknown)
			.WithMessage("At least one field of preferred currency or preferred locale must be provided")
			.WithErrorCode("UPDATE_FIELDS_REQUIRED");
	}
}
