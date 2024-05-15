using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetStockBySymbolQueryHandler : IRequestHandler<GetStockBySymbolQuery, ErrorOr<StockResult>>
{
	private readonly IStockRepository stockRepository;
	private readonly IStockApiClient stockApiClient;

	public GetStockBySymbolQueryHandler(IStockRepository stockRepository, IStockApiClient stockApiClient)
	{
		this.stockRepository = stockRepository;
		this.stockApiClient = stockApiClient;
	}

	public async Task<ErrorOr<StockResult>> Handle(GetStockBySymbolQuery request, CancellationToken cancellationToken)
	{
		var errorOrStock = await this.stockRepository.GetBySymbolAsync(request.Symbol, cancellationToken);

		if (!errorOrStock.IsError)
		{
			return this.MapToStockResult(errorOrStock);
		}

		if (errorOrStock.FirstError is not { Type: ErrorType.NotFound })
		{
			return this.MapToStockResult(errorOrStock);
		}

		errorOrStock = await this.stockApiClient.GetBySymbolAsync(request.Symbol, cancellationToken);

		if (errorOrStock.IsError)
		{
			return this.MapToStockResult(errorOrStock);
		}

		var stock = errorOrStock.Value;

		errorOrStock = await this.stockRepository.AddAsync(
			stock.Symbol,
			stock.Name,
			stock.Region,
			stock.Currency,
			cancellationToken);

		return this.MapToStockResult(errorOrStock);
	}

	private ErrorOr<StockResult> MapToStockResult(ErrorOr<Stock> errorOrStock)
	{
		return errorOrStock.Match(
			stock => new StockResult(
				stock.Id,
				stock.Symbol,
				stock.Name,
				stock.Region,
				stock.Currency),
			errors => (ErrorOr<StockResult>)errors);
	}
}
