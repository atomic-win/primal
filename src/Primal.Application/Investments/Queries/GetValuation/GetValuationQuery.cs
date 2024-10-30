using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetValuationQuery(
	UserId UserId,
	DateOnly Date,
	IReadOnlyCollection<AssetId> AssetIds,
	Currency Currency) : IRequest<ErrorOr<ValuationResult>>;
