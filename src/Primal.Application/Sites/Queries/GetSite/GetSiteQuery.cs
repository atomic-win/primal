using ErrorOr;
using MediatR;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record GetSiteQuery(UserId UserId, SiteId SiteId) : IRequest<ErrorOr<SiteResult>>;
