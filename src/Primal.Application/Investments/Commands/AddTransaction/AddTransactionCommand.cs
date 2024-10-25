using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddTransactionCommand(
	UserId UserId,
	AssetId AssetId,
	DateOnly Date,
	string Name,
	TransactionType Type,
	decimal Units) : IRequest<ErrorOr<TransactionResult>>;
