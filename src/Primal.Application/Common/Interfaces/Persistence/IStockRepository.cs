using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IStockRepository
{
	Task<ErrorOr<Stock>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken);

	Task<ErrorOr<Stock>> GetByIdAsync(StockId id, CancellationToken cancellationToken);

	Task<ErrorOr<Stock>> AddAsync(string symbol, string name, string region, Currency currency, CancellationToken cancellationToken);
}
