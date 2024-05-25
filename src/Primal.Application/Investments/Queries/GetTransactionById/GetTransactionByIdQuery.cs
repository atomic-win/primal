using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetTransactionByIdQuery(UserId UserId, TransactionId TransactionId) : IRequest<ErrorOr<Transaction>>;
