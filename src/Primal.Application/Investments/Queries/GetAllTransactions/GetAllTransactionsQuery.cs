using ErrorOr;
using MediatR;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetAllTransactionsQuery(
	UserId UserId,
	Currency Currency) : IRequest<ErrorOr<IEnumerable<TransactionResult>>>;
