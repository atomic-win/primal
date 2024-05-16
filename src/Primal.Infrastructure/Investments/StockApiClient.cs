using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;

namespace Primal.Infrastructure.Investments;

internal sealed class StockApiClient : IStockApiClient
{
	private readonly HttpClient httpClient;

	public StockApiClient(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public async Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		throw new NotSupportedException();
	}
}
