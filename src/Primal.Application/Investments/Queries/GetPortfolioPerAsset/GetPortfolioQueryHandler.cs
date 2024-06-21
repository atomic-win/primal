using ErrorOr;
using MediatR;

namespace Primal.Application.Investments;

internal sealed class GetPortfolioQueryHandler<T>
	: IRequestHandler<GetPortfolioQuery<T>, ErrorOr<IEnumerable<Portfolio<T>>>>
{
	private readonly PortfolioCalculator portfolioCalculator;

	public GetPortfolioQueryHandler(PortfolioCalculator portfolioCalculator)
	{
		this.portfolioCalculator = portfolioCalculator;
	}

	public async Task<ErrorOr<IEnumerable<Portfolio<T>>>> Handle(GetPortfolioQuery<T> request, CancellationToken cancellationToken)
	{
		return await this.portfolioCalculator.CalculateAsync(
			request.UserId,
			request.Currency,
			request.IdSelector,
			cancellationToken);
	}
}
