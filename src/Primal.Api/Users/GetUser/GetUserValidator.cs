using FastEndpoints;
using FluentValidation;

namespace Primal.Api.Users;

internal sealed class GetUserValidator : Validator<GetUserRequest>
{
	public GetUserValidator()
	{
		this.RuleFor(x => x.UserId)
			.NotEqual(Guid.Empty)
			.WithMessage("User ID must be provided.");
	}
}
