using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddCashTransactionCommand(
	UserId UserId,
	DateOnly Date,
	string Name,
	TransactionType Type,
	AssetId AssetId,
	decimal Amount,
	Currency Currency) : IRequest<ErrorOr<Transaction>>;
