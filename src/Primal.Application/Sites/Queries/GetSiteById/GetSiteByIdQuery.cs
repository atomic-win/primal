using ErrorOr;
using MediatR;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record GetSiteByIdQuery(UserId UserId, SiteId SiteId) : IRequest<ErrorOr<SiteResult>>;
