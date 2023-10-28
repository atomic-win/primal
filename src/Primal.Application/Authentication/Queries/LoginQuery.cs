using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;

namespace Primal.Application.Authentication.Queries;

public sealed record LoginQuery(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;
