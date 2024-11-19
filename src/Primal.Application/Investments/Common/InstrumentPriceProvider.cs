using System.Collections.Immutable;
using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class InstrumentPriceProvider
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;

	public InstrumentPriceProvider(
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
	}

	internal async Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetHistoricalPricesAsync(
		InvestmentInstrument investmentInstrument,
		CancellationToken cancellationToken)
	{
		return investmentInstrument switch
		{
			MutualFund mutualFund => await this.mutualFundApiClient.GetPriceAsync(mutualFund.SchemeCode, cancellationToken),
			Stock stock => await this.stockApiClient.GetPriceAsync(stock.Symbol, cancellationToken),
			_ => ImmutableDictionary<DateOnly, decimal>.Empty,
		};
	}
}
