using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IInstrumentRepository
{
	Task<ErrorOr<IEnumerable<InvestmentInstrument>>> GetAllAsync(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> GetByIdAsync(UserId userId, InstrumentId instrumentId, CancellationToken cancellationToken);

	Task<ErrorOr<MutualFundInstrument>> AddMutualFundAsync(UserId userId, string name, InvestmentCategory category, MutualFundId mutualFundId, CancellationToken cancellationToken);
}
