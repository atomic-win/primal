using ErrorOr;
using MediatR;

namespace Primal.Application.Authentication.Queries;

internal sealed class LoginQueryHandler : IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
	public async Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery request, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return new AuthenticationResult(request.IdToken);
	}
}
