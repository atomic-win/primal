using ErrorOr;
using MediatR;

namespace Primal.Application.Authentication.Commands;

public sealed record RegisterCommand(string IdToken) : IRequest<ErrorOr<AuthenticationResult>>;
