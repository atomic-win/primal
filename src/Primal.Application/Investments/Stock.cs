using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record Stock(
	string Symbol,
	string Name,
	AssetType AssetType,
	Currency Currency);
