using ErrorOr;
using MediatR;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioQueryHandler<T>
	: IRequestHandler<GetPortfolioQuery<T>, ErrorOr<IEnumerable<Portfolio<T>>>>
{
	private readonly InvestmentCalculator investmentCalculator;

	public GetPortfolioQueryHandler(InvestmentCalculator investmentCalculator)
	{
		this.investmentCalculator = investmentCalculator;
	}

	public async Task<ErrorOr<IEnumerable<Portfolio<T>>>> Handle(GetPortfolioQuery<T> request, CancellationToken cancellationToken)
	{
		return await this.investmentCalculator.CalculatePortfolioAsync(
			request.UserId,
			request.Currency,
			request.IdSelector,
			cancellationToken);
	}
}
