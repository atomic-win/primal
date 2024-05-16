using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IMutualFundRepository
{
	Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken);

	Task<ErrorOr<MutualFund>> GetByIdAsync(MutualFundId mutualFundId, CancellationToken cancellationToken);

	Task<ErrorOr<MutualFund>> AddAsync(string schemeName, string fundHouse, string schemeType, string schemeCategory, int schemeCode, Currency currency, CancellationToken cancellationToken);
}
