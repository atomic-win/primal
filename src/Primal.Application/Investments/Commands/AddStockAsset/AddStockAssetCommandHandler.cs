using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddStockAssetCommandHandler : IRequestHandler<AddStockAssetCommand, ErrorOr<Asset>>
{
	private readonly IStockApiClient stockApiClient;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IAssetRepository assetRepository;

	public AddStockAssetCommandHandler(
		IStockApiClient stockApiClient,
		IInstrumentRepository instrumentRepository,
		IAssetRepository assetRepository)
	{
		this.stockApiClient = stockApiClient;
		this.instrumentRepository = instrumentRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<Asset>> Handle(AddStockAssetCommand request, CancellationToken cancellationToken)
	{
		var errorOrStock = await this.GetStockAsync(request.Symbol, cancellationToken);

		if (errorOrStock.IsError)
		{
			return errorOrStock.Errors;
		}

		var stock = errorOrStock.Value;

		return await this.assetRepository.AddAsync(request.UserId, request.Name, stock.Id, cancellationToken);
	}

	private async Task<ErrorOr<InvestmentInstrument>> GetStockAsync(string symbol, CancellationToken cancellationToken)
	{
		var errorOrInvestmentInstrument = await this.instrumentRepository.GetStockBySymbolAsync(symbol, cancellationToken);

		if (!errorOrInvestmentInstrument.IsError)
		{
			return errorOrInvestmentInstrument;
		}

		if (errorOrInvestmentInstrument.IsError && errorOrInvestmentInstrument.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrInvestmentInstrument.Errors;
		}

		var errorOrStock = await this.stockApiClient.GetBySymbolAsync(symbol, cancellationToken);

		if (errorOrStock.IsError)
		{
			return errorOrStock.Errors;
		}

		var stock = errorOrStock.Value;

		return await this.instrumentRepository.AddStockAsync(
			stock.Name,
			stock.Symbol,
			stock.StockType,
			stock.Region,
			stock.MarketOpen,
			stock.MarketClose,
			stock.Timezone,
			stock.Currency,
			cancellationToken);
	}
}
