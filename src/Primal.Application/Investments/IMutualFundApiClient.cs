using Primal.Domain.Money;

namespace Primal.Application.Investments;

public interface IMutualFundApiClient : IAssetApiClient<MutualFund>
{
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "cohesion")]
public sealed record MutualFund(
	string SchemeCode,
	string Name,
	string SchemeType,
	string SchemeCategory,
	Currency Currency);
