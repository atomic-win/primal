using ErrorOr;
using MediatR;
using Primal.Domain.Users;

namespace Primal.Application.Sites;

public sealed record AddSiteCommand(UserId UserId, string Host, int DailyLimitInMinutes) : IRequest<ErrorOr<SiteResult>>;
