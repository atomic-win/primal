using System.Collections.Immutable;
using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;

namespace Primal.Application.Investments;

internal sealed class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>
{
	private readonly IExchangeRateProvider exchangeRateProvider;

	public GetExchangeRatesQueryHandler(IExchangeRateProvider exchangeRateProvider)
	{
		this.exchangeRateProvider = exchangeRateProvider;
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
	{
		if (request.From == request.To)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var errorOrExchangeRates = await this.exchangeRateProvider.GetExchangeRatesAsync(request.From, request.To, cancellationToken);

		if (errorOrExchangeRates.IsError)
		{
			return errorOrExchangeRates.Errors;
		}

		return errorOrExchangeRates.Value
			.Where(kvp => kvp.Key >= request.StartDate && kvp.Key <= request.EndDate)
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}
}
