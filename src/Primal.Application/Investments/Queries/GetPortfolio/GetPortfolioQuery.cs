using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetPortfolioQuery(
	UserId UserId,
	IReadOnlyCollection<AssetId> AssetIds,
	Currency Currency) : IRequest<ErrorOr<IEnumerable<Portfolio>>>;
