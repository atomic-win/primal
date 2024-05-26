using ErrorOr;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Common.Interfaces.Persistence;

public interface IInstrumentRepository
{
	Task<ErrorOr<InvestmentInstrument>> GetByIdAsync(InstrumentId instrumentId, CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> GetCashDepositAsync(CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> GetMutualFundBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> GetStockBySymbolAsync(string symbol, CancellationToken cancellationToken);

	Task<ErrorOr<DateOnly>> GetLatestInstrumentValueDateAsync(InstrumentId instrumentId, CancellationToken cancellationToken);

	Task<ErrorOr<decimal>> GetInstrumentValueAsync(InstrumentId instrumentId, DateOnly date, CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> AddCashDepositAsync(CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> AddMutualFundAsync(
		string name,
		string fundHouse,
		string schemeType,
		string schemeCategory,
		int schemeCode,
		Currency currency,
		CancellationToken cancellationToken);

	Task<ErrorOr<InvestmentInstrument>> AddStockAsync(
		string name,
		string symbol,
		string stockType,
		string region,
		string marketOpen,
		string marketClose,
		string timezone,
		Currency currency,
		CancellationToken cancellationToken);

	Task<ErrorOr<Success>> UpdateInstrumentValueAsync(
		InstrumentId instrumentId,
		DateOnly date,
		decimal value,
		CancellationToken cancellationToken);
}
