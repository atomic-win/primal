using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IMutualFundApiClient
{
	Task<MutualFund> GetBySchemeCodeAsync(string schemeCode, CancellationToken cancellationToken);

	Task<IReadOnlyDictionary<DateOnly, decimal>> GetPriceAsync(string schemeCode, CancellationToken cancellationToken);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "cohesion")]
public sealed record MutualFund(
	string SchemeCode,
	string Name,
	string SchemeType,
	string SchemeCategory,
	Currency Currency);
