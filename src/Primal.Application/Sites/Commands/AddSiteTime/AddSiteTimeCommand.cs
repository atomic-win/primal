using ErrorOr;
using MediatR;
using Primal.Domain.Sites;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record AddSiteTimeCommand(UserId UserId, SiteId SiteId, DateTime Time) : IRequest<ErrorOr<Success>>;
