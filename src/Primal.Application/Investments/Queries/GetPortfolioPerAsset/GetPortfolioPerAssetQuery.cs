using ErrorOr;
using MediatR;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetPortfolioPerAssetQuery(
	UserId UserId,
	Currency Currency) : IRequest<ErrorOr<IEnumerable<PortfolioPerAsset>>>;
