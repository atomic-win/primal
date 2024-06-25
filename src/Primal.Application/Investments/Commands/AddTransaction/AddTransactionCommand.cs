using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddTransactionCommand(
	UserId UserId,
	DateOnly Date,
	string Name,
	TransactionType Type,
	AssetId AssetId,
	decimal Units) : IRequest<ErrorOr<TransactionResult>>;
