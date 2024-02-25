using ErrorOr;
using MediatR;
using Primal.Application.Sites.Common;
using Primal.Domain.Users;

namespace Primal.Application.Sites.Commands;

public sealed record AddSiteCommand(UserId UserId, Uri Url, int DailyLimitInMinutes) : IRequest<ErrorOr<SiteResult>>;
