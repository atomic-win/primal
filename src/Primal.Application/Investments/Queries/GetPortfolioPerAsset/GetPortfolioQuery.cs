using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetPortfolioQuery<T>(
	UserId UserId,
	Currency Currency,
	Func<Transaction, Asset, InvestmentInstrument, T> IdSelector) : IRequest<ErrorOr<IEnumerable<Portfolio<T>>>>;
