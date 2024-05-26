using ErrorOr;
using Primal.Domain.Investments;

namespace Primal.Application.Common.Interfaces.Investments;

public interface IMutualFundApiClient
{
	Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken);

	Task<ErrorOr<IEnumerable<InstrumentValue>>> GetHistoricalValuesAsync(int schemeCode, CancellationToken cancellationToken);
}
