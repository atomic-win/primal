using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IInstrumentRepository
{
	Task<ErrorOr<IEnumerable<Instrument>>> GetAllAsync(UserId userId, CancellationToken cancellationToken);

	Task<ErrorOr<Instrument>> GetByIdAsync(UserId userId, InstrumentId instrumentId, CancellationToken cancellationToken);

	Task<ErrorOr<Instrument>> AddAsync(UserId userId, string name, InvestmentCategory category, InvestmentType type, AccountId accountId, CancellationToken cancellationToken);
}
