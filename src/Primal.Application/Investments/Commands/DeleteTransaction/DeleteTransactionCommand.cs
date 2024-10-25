using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record DeleteTransactionCommand(
	UserId UserId,
	AssetId AssetId,
	TransactionId TransactionId) : IRequest<ErrorOr<Success>>;
