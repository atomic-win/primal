using ErrorOr;
using MediatR;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record GetExchangeRateQuery(
	Currency From,
	Currency To) : IRequest<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>;
