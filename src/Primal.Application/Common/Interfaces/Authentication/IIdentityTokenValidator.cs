using ErrorOr;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Authentication;

public interface IIdentityTokenValidator
{
	Task<ErrorOr<IdentityUser>> Validate(string idToken);
}
