using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Domain.Users;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
	private readonly IIdentityTokenValidator identityTokenValidator;

	public RegisterCommandHandler(IIdentityTokenValidator identityTokenValidator)
	{
		this.identityTokenValidator = identityTokenValidator;
	}

	public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		ErrorOr<IdentityUser> identityUser = await this.identityTokenValidator.Validate(request.IdToken);
		return new AuthenticationResult(request.IdToken);
	}
}
