using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Users;

public sealed record GetUserQuery(UserId UserId) : IRequest<ErrorOr<User>>;
