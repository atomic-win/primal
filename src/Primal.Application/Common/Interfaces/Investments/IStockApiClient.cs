using ErrorOr;
using Primal.Domain.Investments;

namespace Primal.Application.Common.Interfaces.Investments;

public interface IStockApiClient
{
	Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);

	Task<ErrorOr<IReadOnlyDictionary<DateOnly, decimal>>> GetHistoricalValuesAsync(string symbol, CancellationToken cancellationToken);
}
