using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioQueryHandler
	: IRequestHandler<GetPortfolioQuery, ErrorOr<IEnumerable<Portfolio>>>
{
	private readonly InvestmentCalculator investmentCalculator;

	public GetPortfolioQueryHandler(InvestmentCalculator investmentCalculator)
	{
		this.investmentCalculator = investmentCalculator;
	}

	public async Task<ErrorOr<IEnumerable<Portfolio>>> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
	{
		return await this.investmentCalculator.CalculatePortfolioAsync(
			request.UserId,
			request.AssetIds,
			request.Currency,
			cancellationToken);
	}
}
