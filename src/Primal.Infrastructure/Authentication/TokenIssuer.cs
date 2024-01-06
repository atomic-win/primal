using ErrorOr;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Authentication;

internal sealed class TokenIssuer : ITokenIssuer
{
	public async Task<ErrorOr<string>> IssueToken(User user, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return "token";
	}
}
