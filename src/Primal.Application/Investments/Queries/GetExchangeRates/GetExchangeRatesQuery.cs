using ErrorOr;
using MediatR;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record GetExchangeRatesQuery(
	Currency From,
	Currency To,
	DateOnly StartDate,
	DateOnly EndDate) : IRequest<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>;
