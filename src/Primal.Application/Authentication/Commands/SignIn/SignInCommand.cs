using ErrorOr;
using MediatR;

namespace Primal.Application.Authentication;

public sealed record SignInCommand(string IdToken) : IRequest<ErrorOr<SignInResult>>;
