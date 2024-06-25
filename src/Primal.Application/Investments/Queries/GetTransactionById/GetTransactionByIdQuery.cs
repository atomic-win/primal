using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetTransactionByIdQuery(
	UserId UserId,
	Currency Currency,
	TransactionId TransactionId) : IRequest<ErrorOr<TransactionResult>>;
