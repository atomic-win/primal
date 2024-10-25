using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetTransactionsByAssetIdQuery(
	UserId UserId,
	AssetId AssetId,
	Currency Currency) : IRequest<ErrorOr<IEnumerable<TransactionResult>>>;
