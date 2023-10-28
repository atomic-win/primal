using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
	public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return new AuthenticationResult(request.IdToken);
	}
}
