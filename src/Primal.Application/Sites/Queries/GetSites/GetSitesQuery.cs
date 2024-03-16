using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record GetSitesQuery(UserId UserId) : IRequest<ErrorOr<IEnumerable<SiteResult>>>;
