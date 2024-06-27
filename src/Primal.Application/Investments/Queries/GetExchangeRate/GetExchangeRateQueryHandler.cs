using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;

namespace Primal.Application.Investments;

internal sealed class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>>
{
	private readonly IExchangeRateProvider exchangeRateProvider;

	public GetExchangeRateQueryHandler(IExchangeRateProvider exchangeRateProvider)
	{
		this.exchangeRateProvider = exchangeRateProvider;
	}

	public async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
	{
		return await this.exchangeRateProvider.GetExchangeRatesAsync(request.From, request.To, cancellationToken);
	}
}
