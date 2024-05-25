using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetAllTransactionsQuery(UserId UserId) : IRequest<ErrorOr<IEnumerable<Transaction>>>;
