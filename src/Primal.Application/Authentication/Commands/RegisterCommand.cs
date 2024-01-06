using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;

namespace Primal.Application.Authentication.Commands;

public sealed record RegisterCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;
