using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record GetSiteByUrlQuery(UserId UserId, Uri Url) : IRequest<ErrorOr<SiteResult>>;
