using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
	private readonly IIdentityProvider identityProvider;

	public RegisterCommandHandler(IIdentityProvider identityProvider)
	{
		this.identityProvider = identityProvider;
	}

	public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		ErrorOr<IdentityUser> identityUser = await this.identityProvider.Get(request.IdToken);
		return new AuthenticationResult(request.IdToken);
	}
}
