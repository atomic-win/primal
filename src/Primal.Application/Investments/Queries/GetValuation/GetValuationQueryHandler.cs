using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetValuationQueryHandler
	: IRequestHandler<GetValuationQuery, ErrorOr<ValuationResult>>
{
	private readonly InvestmentCalculator investmentCalculator;

	public GetValuationQueryHandler(InvestmentCalculator investmentCalculator)
	{
		this.investmentCalculator = investmentCalculator;
	}

	public async Task<ErrorOr<ValuationResult>> Handle(GetValuationQuery request, CancellationToken cancellationToken)
	{
		return await this.investmentCalculator.CalculateValuationAsync(
			request.UserId,
			request.Date,
			request.AssetIds,
			request.Currency,
			cancellationToken);
	}
}
