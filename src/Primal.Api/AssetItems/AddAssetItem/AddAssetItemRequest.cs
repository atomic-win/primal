using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed record AddAssetItemRequest(
	string Name,
	AssetClass AssetClass,
	AssetType AssetType,
	string ExternalId,
	Currency Currency);
